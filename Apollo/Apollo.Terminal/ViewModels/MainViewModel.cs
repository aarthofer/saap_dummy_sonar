using Apollo.Terminal.Utils;
using Apollo.Terminal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Apollo.Print;
using System.IO;
using System.ComponentModel;

namespace Apollo.Terminal.ViewModels
{
    public class MainViewModel : NotifyPropertyChanged, IApolloViewModel
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ViewManager viewManager;
        private readonly ResetTimer resetTimer;
        private readonly ILogger<MainViewModel> logger;
        private IApolloViewModel currentViewModel;
        public ICommand ShowStartViewCommand { get; private set; }
        public SpinnerService SpinnerService { get; }

        public MainViewModel(IServiceProvider ServiceProvider, ViewManager viewManager, ResetTimer resetTimer, ILogger<MainViewModel> logger, SpinnerService spinner)
        {
            serviceProvider = ServiceProvider ?? throw new ArgumentNullException(nameof(ServiceProvider));
            this.viewManager = viewManager ?? throw new ArgumentNullException(nameof(viewManager));
            this.resetTimer = resetTimer ?? throw new ArgumentNullException(nameof(resetTimer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.SpinnerService = spinner ?? throw new ArgumentNullException(nameof(spinner));

            viewManager.ResetTimer = this.resetTimer;
            viewManager.ViewModelChanged += ViewManager_ViewModelChanged;

            this.SelectGermanCommand = new AsyncDelegateCommand(StartWithGerman, _ => true);
            this.SelectEnglishCommand = new AsyncDelegateCommand(StartWithEnglish, _ => true);
            this.ShowStartViewCommand = new AsyncDelegateCommand(ShowStartView, _ => true);

            viewManager.RegisterViewModel(this);
            viewManager.RegisterViewModel(serviceProvider.GetService<MovieSelectionViewModel>());
            viewManager.RegisterViewModel(serviceProvider.GetService<ReservationViewModel>());
            viewManager.RegisterViewModel(serviceProvider.GetService<CheckoutViewModel>());
            viewManager.RegisterViewModel(serviceProvider.GetService<PurchaseSuccessfullViewModel>());

            Task.Run(() => viewManager.ChangeViewModel<MainViewModel>());

            resetTimer.ResetElapsed += ResetTimer_ResetElapsed;
        }

        private async Task ShowStartView(object arg)
        {
            resetTimer.Reset();
            await this.viewManager.ChangeViewModel<MainViewModel>();
        }

        private void ResetTimer_ResetElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(() => this.viewManager.ChangeViewModel<MainViewModel>());
        }

        public ICommand SelectGermanCommand { get; private set; }
        public ICommand SelectEnglishCommand { get; private set; }

        public IApolloViewModel CurrentViewModel
        {
            get => currentViewModel;
            set => this.Set(ref currentViewModel, value);
        }

        private void ViewManager_ViewModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CurrentViewModel = viewManager.CurrentViewModel;
        }

        private async Task StartWithEnglish(object arg)
        {
            LocalizationHelper.ChangeCurrentLanguage(ApolloLanguages.EN);
            serviceProvider.GetService<MovieSelectionViewModel>();
            await viewManager.ChangeViewModel<MovieSelectionViewModel>();
        }

        private async Task StartWithGerman(object arg)
        {
            LocalizationHelper.ChangeCurrentLanguage(ApolloLanguages.DE);
            serviceProvider.GetService<MovieSelectionViewModel>();
            await viewManager.ChangeViewModel<MovieSelectionViewModel>();
        }
    }
}
