using Apollo.BLInterface;
using Apollo.Core.Domain;
using Apollo.Terminal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Apollo.Terminal.Services;
using Microsoft.Extensions.Logging;

namespace Apollo.Terminal.ViewModels
{
    public class CinemaHallViewModel : NotifyPropertyChanged, IApolloViewModel
    {

        private CinemaHall cinemaHall;
        private ObservableCollection<CinemaHallRowViewModel> rows;
        private int maxRow;
        private int maxCol;

        private Schedule schedule;

        private ILogger<CinemaHallViewModel> Logger { get; }
        public Schedule Schedule { get => schedule; set => Set(ref schedule, value); }
        public CinemaHall CinemaHall { get => cinemaHall; set => Set(ref cinemaHall, value); }
        public ObservableCollection<CinemaHallRowViewModel> Rows { get => rows; set => Set(ref rows, value); }        
        public int MaxRow { get => maxRow; set => Set(ref maxRow, value); }        
        public int MaxCol { get => maxCol; set => Set(ref maxCol, value); }

        private IReservationBl ReservationBl { get; }
        public ICinemaBl CinemaBl { get; }

        public event OnUserSeatInput OnUserSeatInput;


        public CinemaHallViewModel(IReservationBl reservationBl, ICinemaBl cinemaBl, ILogger<CinemaHallViewModel> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ReservationBl = reservationBl ?? throw new ArgumentNullException(nameof(reservationBl));
            CinemaBl = cinemaBl ?? throw new ArgumentNullException(nameof(cinemaBl));
        }

        public async Task InitCinemaHall()
        {
            try
            {
                CinemaHall = await CinemaBl.GetCinemaHall(Schedule.CinemaHallId);
                var seats = await ReservationBl.GetSeatsAsync(schedule.Id);

                MaxCol = seats.Max(seat => seat.Seat.Col);
                MaxRow = seats.Max(seat => seat.Seat.Row);

                seats = seats.OrderBy(seat => seat.Seat.Col);

                var cinemaLayout = new ObservableCollection<CinemaHallRowViewModel>();

                for (int row = maxRow; row > 0; --row)
                {
                    var cinemaHall = new CinemaHallRowViewModel(row, seats.Where(seat => seat.Seat.Row == row), CinemaBl);
                    cinemaHall.OnUserSeatInput += seat => OnUserSeatInput?.Invoke(seat);

                    cinemaLayout.Add(cinemaHall);
                }

                Rows = cinemaLayout;
            } 
            catch (Exception e)
            {
                Logger.LogError(e, "Couldn't fetch seats for given reservation");
                Rows = new ObservableCollection<CinemaHallRowViewModel>();
            }
        }
    }
}
