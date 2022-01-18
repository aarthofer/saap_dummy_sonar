using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Apollo.Terminal.Services
{
    public class TerminalDialogService : IDialogService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
