using Apollo.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface ICinemaHallDao : IApolloDao<CinemaHall>
    {

        Task<int> DeleteCinemaHallAsync(CinemaHall cinemaHall);
        Task<CinemaHallSeat> SaveCinemaHallSeat(CinemaHallSeat seat);
        Task<IEnumerable<CinemaHallSeat>> GetCinemaHallSeats(int cinemaHallId);
        Task<IEnumerable<CinemaHallSeat>> ResetAllSeats(int cinemaHallId, IEnumerable<CinemaHallSeat> seats);

        public Task<IEnumerable<CinemaHall>> FindCinemaHallsByCinemaId(int cinemaId);
    }
}
