using Apollo.BLInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Plugins
{
    class OneReservationPerRow : IConstraint
    {
        public Task<IEnumerable<ReservationSeat>> CalculateConstraints(IEnumerable<ReservationSeat> seats)
        { 
            var reservedSeats = seats.Where(seat => seat.state == ReservationSeat.State.reserved);

            foreach (ReservationSeat currentSeat in reservedSeats)
            {
                var freeSeats = seats.Where(seat => currentSeat.Seat.Row == seat.Seat.Row);

                foreach (var seat in freeSeats)
                {
                    seat.state = ReservationSeat.State.reserved;
                }
            }

            return Task.FromResult(seats);
        }
    }
}
