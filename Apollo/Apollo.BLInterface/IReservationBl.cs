using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IReservationBl
    {
        Task<IEnumerable<ReservationSeat>> GetSeatsAsync(int scheduleId);
        Task<Reservation> AddReservationAsync(Reservation reservation);
        Task<bool> PayReservationAsync(int reservationId);
        Task<bool> PayReservationAsync(Reservation reservation);
        Task<int> RemoveReservationAsync(int reservationId);
        Task<int> RemoveReservationAsync(Reservation reservation);
    }
}
