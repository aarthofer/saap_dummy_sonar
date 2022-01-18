using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface ICinemaBl
    {
        Task<Cinema> GetCinemaByIdAsync(int id);
        Task<Cinema> CreateCinemaAsync(Cinema cinema);
        Task<Cinema> UpdateCinemaAsync(Cinema cinema);
        Task<int> DeleteCinemaAsync(Cinema cinema);
        Task<int> DeleteCinemaAsync(int id);

        Task<SeatCategory> GetCategoryByIdAsync(int id);
        Task<SeatCategory> GetCategoryForCinemaByNameAsync(int cinemaId, string name);
        Task<IEnumerable<SeatCategory>> GetCategoriesForCinemaAsync(int cinemaId);
        Task<SeatCategory> SaveCategoryAsync(SeatCategory category);
        Task<int> DeleteCategoryAsync(SeatCategory category);
        Task<int> DeleteCategoryByIdAsync(int id);

        Task<CinemaHallSeat> SaveCinemaHallSeat(CinemaHallSeat seat);
        Task<IEnumerable<CinemaHallSeat>> GetCinemaHallSeats(int cinemaHallId);
        Task<IEnumerable<CinemaHallSeat>> ResetAllSeats(int cinemaHallId, IEnumerable<CinemaHallSeat> seats);
        Task<CinemaHall> GetCinemaHall(int cinemaHallId);
    }
}
