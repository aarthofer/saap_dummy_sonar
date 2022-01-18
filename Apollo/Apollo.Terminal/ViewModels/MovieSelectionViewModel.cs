using Apollo.BLInterface;
using Apollo.Core.Domain;
using Apollo.Terminal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Apollo.Terminal.Views;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using Apollo.Terminal.Services;

namespace Apollo.Terminal.ViewModels
{
    public class MovieSelectionViewModel : NotifyPropertyChanged, IApolloViewModel
    {
        private DateTime currentDate = DateTime.Now;
        private string searchActor;
        private string searchTitle;
        private string searchGenre;
        private readonly IServiceProvider serviceProvider;
        private readonly IMovieBl movieBl;
        private readonly IScheduleBl scheduleBl;

        private ILogger<MovieSelectionViewModel> Logger { get; }

        public ICommand PrevDayCommand { get; private set; }
        public ICommand NextDayCommand { get; private set; }
        public ICommand TodayCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand ResetFilterCommand { get; private set; }

        public string Title { get; set; }

        public ObservableCollection<ScheduleViewModel> ScheduleOfTheDay { get; set; } = new ObservableCollection<ScheduleViewModel>();
        public ICollectionView ScheduleOfTheDayView { get; set; }
        public string SearchActor
        {
            get => this.searchActor;

            set
            {
                this.Set(ref this.searchActor, value);
                this.Filter();
            }
        }
        public string SearchGenre
        {
            get => this.searchGenre;

            set
            {
                this.Set(ref this.searchGenre, value);
                this.Filter();
            }
        }
        public string SearchTitle
        {
            get => this.searchTitle;

            set
            {
                this.Set(ref this.searchTitle, value);
                this.Filter();
            }
        }

        private void Filter()
        {
            ScheduleOfTheDayView.Refresh();
        }

        public string CurrentDateStr
        {
            get
            {
                return CurrentDate.ToString(Application.Current.Resources["strDateFormat"].ToString());
            }
        }
        public DateTime CurrentDate
        {
            get => currentDate;
            set
            {
                this.Set(ref currentDate, value);
                this.OnPropertyChanged(nameof(CurrentDateStr));
                Task.Run(async () => await RefreshScheduleOfTheDay());
            }
        }

        public async Task BeforeShowViewModel()
        {
            await Task.Run(() => RefreshScheduleOfTheDay());
        }

        private async Task RefreshScheduleOfTheDay()
        {
            try
            {
                serviceProvider.GetService<SpinnerService>().Busy = true;
                serviceProvider.GetService<SpinnerService>().Message = Application.Current.Resources["strSpinnerLoadingSchedule"].ToString();
                var schedule = await scheduleBl.GetSchedulesOfDay(CurrentDate, 1);

                if (Thread.CurrentThread != System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread)
                {
                    UpdateScheduleOfTheDay(schedule);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => UpdateScheduleOfTheDay(schedule));
                }
                serviceProvider.GetService<SpinnerService>().Busy = false;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while updating schedule of the day");
                serviceProvider.GetService<SpinnerService>().Busy = false;
                await serviceProvider.GetService<ViewManager>().ChangeViewModel<MainViewModel>();
                serviceProvider.GetService<IDialogService>().ShowMessage(Application.Current.Resources["strMajorError"].ToString());
            }
        }

        private void UpdateScheduleOfTheDay(IEnumerable<Schedule> schedule)
        {
            ScheduleOfTheDay.Clear();
            foreach (var item in schedule)
            {
                ScheduleOfTheDay.Add(new ScheduleViewModel(serviceProvider, serviceProvider.GetService<IReservationBl>(), serviceProvider.GetService<ILogger<ScheduleViewModel>>(), serviceProvider.GetService<IMovieBl>(), item));
            }
        }

        public MovieSelectionViewModel(IServiceProvider serviceProvider, IScheduleBl scheduleBl, IMovieBl movieBl, ILogger<MovieSelectionViewModel> logger)
        {
            this.movieBl = movieBl ?? throw new ArgumentNullException(nameof(movieBl));
            this.scheduleBl = scheduleBl ?? throw new ArgumentNullException(nameof(scheduleBl));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            serviceProvider.GetService<ResetTimer>().ResetElapsed += MovieSelectionViewModel_ResetToLanguageSelection;
            serviceProvider.GetService<PurchaseSuccessfullViewModel>().PurchaseDoneReset += MovieSelectionViewModel_ResetToLanguageSelection;

            this.PrevDayCommand = new AsyncDelegateCommand(PrevDay, PrevDayCanExec);
            this.NextDayCommand = new AsyncDelegateCommand(NextDay, NextDayCanExec);
            this.TodayCommand = new AsyncDelegateCommand(Today);
            this.SearchCommand = new AsyncDelegateCommand(Search);
            this.ResetFilterCommand = new AsyncDelegateCommand(ResetFilter);

            ScheduleOfTheDayView = CollectionViewSource.GetDefaultView(ScheduleOfTheDay);
            ScheduleOfTheDayView.Filter = filter =>
            {
                ScheduleViewModel schedule = (ScheduleViewModel)filter;
                Movie movie = movieBl.GetMovieByIdAsync(schedule.Schedule.MovieId).Result;
                return movie.Title.Contains(SearchTitle ?? "", StringComparison.OrdinalIgnoreCase) &&
                       movie.Actors.Any(a => a.Name.Contains(SearchActor ?? "", StringComparison.OrdinalIgnoreCase)) &&
                       movie.Genre.Any(g => g.Name.Contains(SearchGenre ?? "", StringComparison.OrdinalIgnoreCase));
            };
            ScheduleOfTheDayView.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Ascending));
        }

        private void MovieSelectionViewModel_ResetToLanguageSelection(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Write directly to current Date to avoid reload of schedule
            currentDate = DateTime.Now;
        }

        private Task ResetFilter(object arg)
        {
            SearchTitle = "";
            SearchActor = "";
            SearchGenre = "";
            Filter();
            return Task.CompletedTask;
        }

        private async Task Search(object arg)
        {
        }

        private async Task Today(object arg)
        {
            serviceProvider.GetService<SpinnerService>().Busy = true;
            await Task.Run(() => CurrentDate = DateTime.Now);
            serviceProvider.GetService<SpinnerService>().Busy = false;
        }

        private bool NextDayCanExec(object obj)
        {
            return true;
            return DateTime.Now.Date.AddDays(10) > CurrentDate;
        }

        private bool PrevDayCanExec(object obj)
        {
            return true;
            return CurrentDate > DateTime.Now;
        }

        private async Task NextDay(object arg)
        {
            serviceProvider.GetService<SpinnerService>().Busy = true;
            await Task.Run(() => CurrentDate = CurrentDate.AddDays(1));
        }

        private async Task PrevDay(object arg)
        {
            serviceProvider.GetService<SpinnerService>().Busy = true;
            await Task.Run(() => CurrentDate = CurrentDate.AddDays(-1));
        }
    }
}
