using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Terminal.Utils
{
    public class ViewManager
    {
        private List<IApolloViewModel> viewModels;

        public event PropertyChangedEventHandler ViewModelChanged;

        public IApolloViewModel CurrentViewModel { get; private set; }

        public ResetTimer ResetTimer { get; set; }

        public IServiceProvider ServiceProvider { get; }
        public ViewManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public List<IApolloViewModel> ViewModels
        {
            get
            {
                if (viewModels == null)
                    viewModels = new List<IApolloViewModel>();

                return viewModels;
            }
        }


        public void RegisterViewModel(IApolloViewModel viewModel)
        {
            ViewModels.Add(viewModel);
        }

        internal async Task ChangeViewModel<T>(IApolloViewModel notRegisteredViewModel = null) where T : IApolloViewModel
        {
            IApolloViewModel model;
            if (notRegisteredViewModel != null)
            {
                model = notRegisteredViewModel;
            }
            else
            {
                model = ViewModels.FirstOrDefault(x => x.GetType().Equals(typeof(T)));
            }

            if (model == null)
            {
                throw new ArgumentException("Unknown ViewModel");
            }

            CurrentViewModel = model;
            ResetTimer.Reset();
            await CurrentViewModel.BeforeShowViewModel();
            ViewModelChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
        }
    }
}
