using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Terminal.ViewModels
{
    public class MovieCrewViewModel
    {
        public MovieCrewViewModel(string role, string name)
        {
            Role = role;
            Name = name;
        }

        public string Role { get; set; }
        public string Name { get; set; }
    }
}
