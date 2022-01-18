using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Apollo.Terminal.Utils
{
    public class ResetTimer
    {
        private readonly ILogger<ResetTimer> logger;
        private readonly IConfiguration configuration;
        private Timer timer;

        public event ElapsedEventHandler ResetElapsed;

        public ResetTimer(ILogger<ResetTimer> logger, IConfiguration configuration)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.timer = new Timer(GetTimerInterval());
            this.timer.AutoReset = true;
            this.timer.Elapsed += Timer_Elapsed;
        }

        public void Reset()
        {
            this.logger.LogInformation("Reset Timer!");
            this.timer.Stop();
            this.timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetElapsed?.Invoke(this, e);
        }

        private int GetTimerInterval()
        {
            try
            {
                int val = this.configuration.GetValue<int>(ViewConst.TIMER_INTERVAL_KEY);
                this.logger.LogInformation($"Interval: {val}");
                return val > 0 ? val : ViewConst.TIMER_INTERVAL_DEFAULT;
            }
            catch (Exception e)
            {
                this.logger.LogWarning(e, $"Error reading reset interval, use default {ViewConst.TIMER_INTERVAL_DEFAULT}");
                return ViewConst.TIMER_INTERVAL_DEFAULT;
            }
        }
    }
}
