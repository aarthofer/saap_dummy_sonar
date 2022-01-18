using Apollo.BLInterface;
using Apollo.Terminal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Apollo.Terminal.ViewModels
{

    public class CinemaHallRowViewModel: NotifyPropertyChanged, IApolloViewModel
    {
        private int rowNr;

        public event OnUserSeatInput OnUserSeatInput;

        public ObservableCollection<ReservationSeatViewModel> Row { get; set; } = new ObservableCollection<ReservationSeatViewModel>();
        public int RowNumber { get => rowNr; set => Set(ref rowNr, value); }

        public CinemaHallRowViewModel(int rowNr, IEnumerable<ReservationSeat> row, ICinemaBl cinemaBl)
        {
            RowNumber = rowNr;

            Row.Clear();
            int currentCol = 0;
            foreach (var seat in row)
            {
                if (!IsInNextCol(ref currentCol, seat))
                {
                    Row.Add(new ReservationSeatViewModel(new ReservationSeat { state = ReservationSeat.State.corridor }, null));
                }

                var seatViewModel = new ReservationSeatViewModel(seat, cinemaBl.GetCategoryByIdAsync(seat.Seat.CategoryId).Result);
                seatViewModel.OnUserSeatInput += seat => OnUserSeatInput.Invoke(seat);
                Row.Add(seatViewModel);
            }
        }


        private bool IsInNextCol(ref int currentCol, ReservationSeat seat)
        {
            return seat.Seat.Col == ++currentCol;
        }
    }
}
