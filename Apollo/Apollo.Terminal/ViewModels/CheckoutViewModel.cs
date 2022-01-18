using Apollo.BLInterface;
using Apollo.Core.Domain;
using Apollo.Pay;
using Apollo.Print;
using Apollo.Terminal.Services;
using Apollo.Terminal.Utils;
using Apollo.Terminal.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace Apollo.Terminal.ViewModels
{
    public delegate void CheckoutFinished();
    public class CheckoutViewModel : NotifyPropertyChanged, IApolloViewModel
    {
        public event CheckoutFinished OnCheckoutFinished;

        public ObservableCollection<ReservationSeatViewModel> CurrentReservation { get; set; } = new ObservableCollection<ReservationSeatViewModel>();
        private Schedule schedule;
        public Schedule Schedule { get => schedule; private set => Set(ref schedule, value); }
        public string StartDateTime { get => Schedule.StartTime.ToString(Application.Current.Resources["strDateTimeFormat"].ToString(), LocalizationHelper.CurrentCulture); }

        public string ScheduleStartTime { get => Schedule.StartTime.ToString(Application.Current.Resources["strDateTimeFormat"].ToString(), LocalizationHelper.CurrentCulture); }

        private bool error;
        public bool Error { get => error; private set => Set(ref error, value); }

        private decimal currentPrice;
        public decimal CurrentPrice { get => currentPrice; private set => Set(ref currentPrice, value); }

        public CreditCard CreditCard { get; set; } = new CreditCard();
        public int ExpirationYear { get; set; } = 0;
        public int ExpirationMonth { get; set; } = 0;
        public IServiceProvider ServiceProvider { get; }
        public ICommand FinishCheckoutCommand { get; set; }
        public ICommand NavigateBackCommand { get; set; }

        private Pay.IPayment ApolloPay { get; }

        private const int TERMINAL_USERID = 1;

        private ILogger<CheckoutViewModel> Logger { get; }

        public CheckoutViewModel(IServiceProvider serviceProvider, ILogger<CheckoutViewModel> logger)
        {
            ServiceProvider = serviceProvider;
            FinishCheckoutCommand = new AsyncDelegateCommand(FinishCheckout);
            NavigateBackCommand = new AsyncDelegateCommand(NavigateBack);
            ApolloPay = ServiceProvider.GetService<Pay.IPayment>();
            OnCheckoutFinished += ResetState;

            ServiceProvider.GetService<ResetTimer>().ResetElapsed += ResetTimerElapsed;

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void ResetTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => ResetState());
        }

        private async Task NavigateBack(object arg)
        {
            ResetState();
            await ServiceProvider.GetService<ViewManager>().ChangeViewModel<ReservationViewModel>();
        }

        private async Task FinishCheckout(object arg)
        {
            ServiceProvider.GetService<SpinnerService>().Busy = true;
            await Task.Run(async () => await doCheckout());
        }

        private async Task doCheckout()
        {
            ServiceProvider.GetService<SpinnerService>().Message = Application.Current.Resources["strSpinnerCheckReservation"].ToString();
            CurrentPrice = (decimal)CurrentReservation.Sum(res => res.Category.Price);
            Reservation reservation = new Reservation();
            try
            {
                reservation = CreateReservation();
                ServiceProvider.GetService<SpinnerService>().Message = Application.Current.Resources["strSpinnerPayment"].ToString();
                reservation.IsPayed = await MakePayment();

                reservation = await ServiceProvider.GetService<IReservationBl>().AddReservationAsync(reservation);
                var cinema = await ServiceProvider.GetService<ICinemaBl>().GetCinemaHall(
                      (await ServiceProvider.GetService<ICinemaBl>().GetCinemaHall(Schedule.CinemaHallId)).CinemaId);

                Movie movie = await ServiceProvider.GetService<IMovieBl>().GetMovieByIdAsync(Schedule.MovieId);
                var printerService = ServiceProvider.GetService<IPrint>();

                ServiceProvider.GetService<SpinnerService>().Message = Application.Current.Resources["strSpinnerPrintingTickets"].ToString();
                foreach (var seat in reservation.Seats)
                {
                    var ticket = new TicketInformation
                    {
                        Cinema = cinema.Name,
                        MovieTitle = movie.Title,
                        ReservatoinId = reservation.Id,
                        Row = $"{seat.Row}",
                        Seat = $"{seat.SeatNr}",
                        StartTime = Schedule.StartTime
                    };
                    var printer = ServiceProvider.GetService<PrintService>();

                    await printer.PrintTicket(ticket);
                }

                OnCheckoutFinished?.Invoke();

                await ServiceProvider.GetService<ViewManager>().ChangeViewModel<PurchaseSuccessfullViewModel>();
                ServiceProvider.GetService<SpinnerService>().Busy = false;
            }
            catch (PaymentException pE)
            {
                Logger.LogError(pE, "Error in payment service");
                switch (pE.Result)
                {
                    case PaymentResult.CardExpired: ServiceProvider.GetService<IDialogService>().ShowMessage(Application.Current.Resources["CardExpiredException"].ToString()); break;
                    case PaymentResult.CardReportedLost: ServiceProvider.GetService<IDialogService>().ShowMessage(Application.Current.Resources["CardReportedLostException"].ToString()); break;
                    case PaymentResult.InsufficientFunds: ServiceProvider.GetService<IDialogService>().ShowMessage(Application.Current.Resources["InsufficientFundsException"].ToString()); break;
                    case PaymentResult.InvalidCardValidationCode: ServiceProvider.GetService<IDialogService>().ShowMessage(Application.Current.Resources["InvalidCardValidationCodeException"].ToString()); break;
                    case PaymentResult.InvalidName: ServiceProvider.GetService<IDialogService>().ShowMessage(Application.Current.Resources["InvalidNameException"].ToString()); break;
                    default: ServiceProvider.GetService<IDialogService>().ShowMessage(pE.Message); break;
                }
                ServiceProvider.GetService<SpinnerService>().Busy = false;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while checkout");
                //Remove Reservation again if there was an error
                if (reservation.Id != -1)
                {
                    Logger.LogInformation("Error while adding reservation. Delete again if already added to DB");
                    try
                    {
                        await ServiceProvider.GetService<IReservationBl>().RemoveReservationAsync(reservation);
                    }
                    catch (Exception innerException)
                    {
                        Logger.LogError(innerException, "Couldn't delete Reservation from DB again");
                        ServiceProvider.GetService<IDialogService>().ShowMessage(innerException.Message);
                    }
                }

                ServiceProvider.GetService<IDialogService>().ShowMessage(e.Message);
                ServiceProvider.GetService<SpinnerService>().Busy = false;
            }
        }

        private Reservation CreateReservation()
        {
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            foreach (var res in CurrentReservation)
            {
                seats.Add(res.Seat);
            }

            return new Reservation { ScheduleId = Schedule.Id, Seats = seats, UserId = TERMINAL_USERID };
        }

        private async Task<bool> MakePayment()
        {
            try
            {
                if (ExpirationYear == 0 || ExpirationMonth <= 0 || ExpirationMonth > 12)
                {
                    throw new PaymentException(PaymentResult.InvalidPaymentResult, Application.Current.Resources["CardDateException"].ToString());
                }

                CreditCard.ExpirationDate = new ExpirationDate(ExpirationMonth, ExpirationYear);

                if (!ApolloPay.ValidateCreditCardNumber(CreditCard))
                {
                    throw new PaymentException(PaymentResult.InvalidPaymentResult, Application.Current.Resources["CardInvalidException"].ToString());
                }

                PaymentResult result = await ApolloPay.CreateTransactionAsync(CurrentPrice, CreditCard, Application.Current.Resources["strPaymentDone"].ToString());

                if (result != PaymentResult.PaymentSuccessful)
                {
                    throw new PaymentException(result, "Payment not successfull");
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while making payment");
                throw e;
            }
        }

        private void ResetState()
        {
            Error = false;
            Schedule = null;
            ExpirationMonth = 0;
            ExpirationYear = 0;
            CurrentPrice = 0;
            Application.Current.Dispatcher.Invoke(() => CurrentReservation.Clear());
            CreditCard = new CreditCard();
        }

        public void Init(ObservableCollection<ReservationSeatViewModel> reservationSeats, Schedule schedule)
        {
            ServiceProvider.GetService<SpinnerService>().Busy = true;
            Application.Current.Dispatcher.Invoke(() => CurrentReservation.Clear());

            foreach (var res in reservationSeats)
            {
                CurrentReservation.Add(res);
            }

            CurrentPrice = (decimal)CurrentReservation.Sum(res => res.Category.Price);
            Schedule = schedule;

            ServiceProvider.GetService<SpinnerService>().Busy = false;
        }

    }
}
