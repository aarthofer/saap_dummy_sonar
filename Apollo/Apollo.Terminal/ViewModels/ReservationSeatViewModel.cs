using Apollo.BLInterface;
using Apollo.Core.Domain;
using Apollo.Terminal.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Apollo.Terminal.ViewModels
{
    public class ReservationSeatViewModel : NotifyPropertyChanged, IApolloViewModel
    {

        private ReservationSeat ReservationSeat;
        
        public int? SeatNr { get => ReservationSeat?.Seat?.SeatNr; }
        public int? RowNr { get => ReservationSeat?.Seat?.Row; }

        public SeatCategory Category { get; }
        public CinemaHallSeat Seat { get => ReservationSeat?.Seat; }

        public ICommand UserSeatInputCommand { get; private set; }

        public event OnUserSeatInput OnUserSeatInput;

        public ReservationSeatViewModel(IServiceProvider serviceProvider)
        {
            serviceProvider.GetService<CheckoutViewModel>().OnCheckoutFinished += () => ReservationSeat = null;
        }
        public ReservationSeat.State State { get => ReservationSeat.state; 
            set 
            {
                ReservationSeat.state = value;
                OnPropertyChanged();
            } 
        }
        public ReservationSeatViewModel(ReservationSeat reservationSeat, SeatCategory category)
        {
            Category = category;
            ReservationSeat = reservationSeat;

            UserSeatInputCommand = new AsyncDelegateCommand(onUserSeatInput);
        }

        private async Task onUserSeatInput(object param)
        {
            if (!(param is ReservationSeatViewModel)) { return; }

            ReservationSeatViewModel seat = (ReservationSeatViewModel)param;

            OnUserSeatInput?.Invoke(seat);
        }


    }
}
