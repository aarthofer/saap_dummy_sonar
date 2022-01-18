using Apollo.Terminal.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Apollo.Terminal.ViewModels
{
    public class PurchaseSuccessfullViewModel : IApolloViewModel
    {
        public event ElapsedEventHandler PurchaseDoneReset;
        private Timer timer;
        private const int DEFAULT_SUCCESS_PAGE_SECONDS = 5000;

        public IServiceProvider ServiceProvider { get; }

        public async Task BeforeShowViewModel() 
        {
            this.timer.Stop();
            this.timer.Start();
            this.timer.Elapsed += Timer_Elapsed;
        }

        public PurchaseSuccessfullViewModel(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            this.timer = new Timer(GetTimerInterval());
            this.timer.AutoReset = true;
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PurchaseDoneReset?.Invoke(this, e);
            await ServiceProvider.GetService<ViewManager>().ChangeViewModel<MainViewModel>();
            this.timer.Stop();
        }

        private int GetTimerInterval()
        {
            return DEFAULT_SUCCESS_PAGE_SECONDS;
        }
    }
}
