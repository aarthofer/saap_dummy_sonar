using Apollo.BLInterface;
using Apollo.Core.Domain;
using Apollo.Terminal.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Timers;

namespace Apollo.Terminal.ViewModels
{
    public delegate void OnUserSeatInput(ReservationSeatViewModel seat);
    public class ReservationViewModel : NotifyPropertyChanged, IApolloViewModel
    {

        private Schedule schedule;
        private CinemaHallViewModel currentCinemaHall;
        private double currentPrice = 0;

        public ICommand CheckoutCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        public Cinema Cinema { get; set; }
        public Movie CurrentMovie { get; set; }
        public CinemaHallViewModel CurrentCinemaHall { get => currentCinemaHall; set => Set(ref currentCinemaHall, value); }
        public ObservableCollection<ReservationSeatViewModel> CurrentReservation { get; set; } = new ObservableCollection<ReservationSeatViewModel>();

        public double CurrentPrice { get => currentPrice; private set => Set(ref currentPrice, value); }

        private IServiceProvider ServiceProvider { get; }

        public Schedule Schedule
        {
            get => schedule;
            set
            {
                this.Set(ref this.schedule, value);
                this.OnPropertyChanged(nameof(ScheduleStartTime));
                return;
            }
        }
        public string ScheduleStartTime
        {
            get
            {
                if (Schedule == null) { return ""; }
                return Schedule.StartTime.ToString(Application.Current.Resources["strDateTimeFormat"].ToString(), LocalizationHelper.CurrentCulture);
            }
        }
        private async Task handleUserSeatInput(ReservationSeatViewModel seat)
        {
            switch (seat.State)
            {
                case ReservationSeat.State.free:
                    seat.State = ReservationSeat.State.selected;
                    CurrentReservation.Add(seat);
                    break;

                case ReservationSeat.State.selected:
                    seat.State = ReservationSeat.State.free;
                    CurrentReservation.Remove(seat);
                    break;

                case ReservationSeat.State.reserved:
                    if (!CurrentReservation.Contains(seat))
                    {
                        return;
                    }
                    CurrentReservation.Remove(seat);
                    seat.State = ReservationSeat.State.free;
                    break;

                default:
                    return;
            }

            UpdateCurrentPrice();
        }

        public ReservationViewModel(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            CheckoutCommand = new AsyncDelegateCommand(doCheckout);
            BackCommand = new AsyncDelegateCommand(goBack);
            ServiceProvider.GetService<CinemaHallViewModel>().OnUserSeatInput += async seat => await handleUserSeatInput(seat);
            CurrentCinemaHall = ServiceProvider.GetService<CinemaHallViewModel>();


            ServiceProvider.GetService<CheckoutViewModel>().OnCheckoutFinished += ResetCurrentReservation;
            ServiceProvider.GetService<ResetTimer>().ResetElapsed += ResetReservation_NonUIThread;
        }

        private void ResetReservation_NonUIThread(object sender, ElapsedEventArgs e)
        {
            ResetCurrentReservation();

        }

        private async Task goBack(object arg)
        {
            await ServiceProvider.GetService<ViewManager>().ChangeViewModel<MovieSelectionViewModel>();
            ResetCurrentReservation();
        }

        private async Task doCheckout(object arg)
        {
            CheckoutViewModel checkout = ServiceProvider.GetService<CheckoutViewModel>();
            checkout.Init(CurrentReservation, Schedule);

            await ServiceProvider.GetService<ViewManager>().ChangeViewModel<CheckoutViewModel>();
        }

        public void ResetCurrentReservation()
        {
            foreach (var Seat in CurrentReservation)
            {
                Seat.State = ReservationSeat.State.free;
            }

            Application.Current.Dispatcher.Invoke(() => CurrentReservation.Clear());
            CurrentPrice = 0;
            Schedule = null;
        }

        private void UpdateCurrentPrice()
        {
            CurrentPrice = Math.Round(CurrentReservation.Sum(res => res.Category.Price), 5);
        }

        public async Task InitReservationViewModel(Schedule schedule)
        {
            await Task.Run(async () =>
            {
                Application.Current.Dispatcher.Invoke(() => ResetCurrentReservation());
                Schedule = schedule;
                if (Schedule == null) { return; }

                CurrentCinemaHall.Schedule = Schedule;
                await CurrentCinemaHall.InitCinemaHall();
                Cinema = await ServiceProvider.GetService<ICinemaBl>().GetCinemaByIdAsync(CurrentCinemaHall.CinemaHall.CinemaId);
                CurrentMovie = await ServiceProvider.GetService<IMovieBl>().GetMovieByIdAsync(Schedule.MovieId);
            });
        }
    }
}
