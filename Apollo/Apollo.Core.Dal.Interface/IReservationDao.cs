using Apollo.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IReservationDao : IApolloDao<Reservation>
    {
        Task<IEnumerable<Reservation>> GetReservations(int scheduleId);
        Task<Reservation> AddSeatToReservation(Reservation reservation, CinemaHallSeat seat);

        Task<Reservation> AddSeatsToReservation(Reservation reservation, IEnumerable<CinemaHallSeat> seats);

        Task<Reservation> RemoveSeatFromReservation(Reservation reservation, CinemaHallSeat seat);
    }
}
