using Apollo.Core.Domain;
using Apollo.Terminal.Utils;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Apollo.BLInterface;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Windows.Media;
using System.Diagnostics;
using System.Web;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows;
using System.Collections.ObjectModel;
using Apollo.Terminal.Services;

namespace Apollo.Terminal.ViewModels
{
    public class ScheduleViewModel : NotifyPropertyChanged, IApolloViewModel
    {
        private const string DEFAULT_IMAGE_NAME = "default.png";

        private readonly IServiceProvider serviceProvider;
        private readonly IReservationBl reservationBl;
        private readonly IMovieBl movieBl;
        private readonly ILogger<ScheduleViewModel> logger;

        private string seatsFree = "0 / 0";
        private ImageSource movieImage = new BitmapImage();

        public ICommand ShowReservationScreenCommand { get; private set; }
        public ICommand ShowDetailScreenCommand { get; private set; }
        public ICommand BackCommand { get; private set; }

        public ICommand Hyperlink_RequestNavigate { get; set; }

        public Schedule Schedule { get; set; }
        public Movie ScheduleMovie { get; set; }
        public CinemaHall ScheduleCinemaHall { get; set; }
        public string StartDate { get => Schedule.StartTime.Date.ToString(Application.Current.Resources["strDateFormat"].ToString()); }
        public string StartTime { get => Schedule.StartTime.TimeOfDay.ToString(); }
        public string StartDateTime { get => Schedule.StartTime.ToString(Application.Current.Resources["strDateTimeFormat"].ToString(), LocalizationHelper.CurrentCulture); }
        public string SeatsFree
        {
            get => seatsFree;
            set
            {
                Set(ref seatsFree, value);
            }
        }
        public string CinemaHall { get => ScheduleCinemaHall.Name; }
        public string ReleaseDuration { get => ScheduleMovie.ReleaseDate.ToString(Application.Current.Resources["strDateFormat"].ToString()) + " | " + ScheduleMovie.DurationMinutes +" Min."; }
        public ImageSource MovieImage { get => movieImage; set => Set(ref movieImage, value); }

        public ObservableCollection<MovieCrewViewModel> MovieCrew { get; set; } = new ObservableCollection<MovieCrewViewModel>();

        public ScheduleViewModel(IServiceProvider serviceProvider, IReservationBl reservationBl, ILogger<ScheduleViewModel> logger, IMovieBl movieBl, Schedule schedule)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.reservationBl = reservationBl ?? throw new ArgumentNullException(nameof(reservationBl));
            this.movieBl = movieBl ?? throw new ArgumentNullException(nameof(reservationBl));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ShowReservationScreenCommand = new AsyncDelegateCommand(ShowReservation);
            this.ShowDetailScreenCommand = new AsyncDelegateCommand(ShowDetails);
            this.BackCommand = new AsyncDelegateCommand(GoBack);
            this.Hyperlink_RequestNavigate = new AsyncDelegateCommand(OpenTrailer);

            this.Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            this.ScheduleMovie = movieBl.GetMovieByIdAsync(Schedule.MovieId).Result;
            this.ScheduleCinemaHall = serviceProvider.GetService<ICinemaBl>().GetCinemaHall(Schedule.CinemaHallId).Result;
            CreateMovieCrewCollection();
        }

        private void CreateMovieCrewCollection()
        {
            foreach (var actors in ScheduleMovie.Actors)
            {
                string val = Application.Current.Resources["strActor"].ToString();
                MovieCrew.Add(new MovieCrewViewModel(val, actors.Name));
            }
            foreach (var crew in ScheduleMovie.Crew)
            {
                MovieCrew.Add(new MovieCrewViewModel(crew.Value, crew.Key.Name));
            }
        }

        private Task OpenTrailer(object arg)
        {
            try
            {
                _ = new Uri(ScheduleMovie.Trailer);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Invalid Trailer - URL");
                return Task.CompletedTask;
            }

            var ps = new ProcessStartInfo(ScheduleMovie.Trailer)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
            return Task.CompletedTask;
        }

        public async Task LoadReservationState()
        {
            logger.LogInformation("xxx: " + Schedule.Id);
            try
            {
                var seats = await reservationBl.GetSeatsAsync(Schedule.Id);
                SeatsFree = $"{seats.Where(s => s.state == ReservationSeat.State.free).Count()} / {seats.Count()}";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading reservation state");
            }
            logger.LogInformation("yyy: " + Schedule.Id);
        }

        private async Task LoadImage()
        {
            if (Thread.CurrentThread != System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread)
            {
                await AttachImageSource();
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await AttachImageSource();
                });
            }
        }

        private async Task AttachImageSource()
        {
            using (BinaryWriter bw = new BinaryWriter(new BufferedStream(new MemoryStream())))
            {
                try
                {
                    await serviceProvider.GetService<IImageService>().GetImageAsync(ScheduleMovie.Image, bw);
                }catch(Exception ex)
                {
                    logger.LogWarning(ex, "Image not found, loading default image");
                    await serviceProvider.GetService<IImageService>().GetImageAsync(DEFAULT_IMAGE_NAME, bw);
                }

                bw.BaseStream.Position = 0;
                var imageSource = new BitmapImage();
                using (BinaryReader br = new BinaryReader(bw.BaseStream))
                {
                    imageSource.BeginInit();
                    imageSource.StreamSource = new MemoryStream(br.ReadBytes((int)bw.BaseStream.Length));
                    imageSource.EndInit();
                }
                MovieImage = imageSource;
            }
        }

        private async Task ShowReservation(object scheduleParameter)
        {
            serviceProvider.GetService<SpinnerService>().Busy = true;
            serviceProvider.GetService<SpinnerService>().Message = Application.Current.Resources["strSpinnerLoadingCinemaHall"].ToString();
            await Task.Run(async () => await serviceProvider.GetService<ReservationViewModel>().InitReservationViewModel(Schedule));
            await Task.Run(async () => await serviceProvider.GetService<ViewManager>().ChangeViewModel<ReservationViewModel>());
            serviceProvider.GetService<SpinnerService>().Busy = false;
        }

        private async Task ShowDetails(object arg)
        {
            serviceProvider.GetService<SpinnerService>().Busy = true;
            serviceProvider.GetService<SpinnerService>().Message = Application.Current.Resources["strSpinnerLoadingDetails"].ToString();
            await Task.Run(async () => await LoadReservationState());
            await Task.Run(async () => await LoadImage());
            await this.serviceProvider.GetService<ViewManager>().ChangeViewModel<ScheduleViewModel>(this);
            serviceProvider.GetService<SpinnerService>().Busy = false;
        }

        private async Task GoBack(object arg)
        {
            await serviceProvider.GetService<ViewManager>().ChangeViewModel<MovieSelectionViewModel>();
        }
    }
}
