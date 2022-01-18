using Apollo.Terminal.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Terminal.Services
{
    public class SpinnerService : NotifyPropertyChanged
    {
        private String message;
        public String Message { get => message; set => Set(ref message, value); }

        private bool busy = false;
        public bool Busy
        {
            get => busy; 
            set
            {
                Set(ref busy, value);
                Message = null;
            }
        }

    }
}
