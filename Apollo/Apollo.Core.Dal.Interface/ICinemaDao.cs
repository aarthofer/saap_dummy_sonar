using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface ICinemaDao : IApolloDao<Cinema>
    {
        Task<int> RemoveCinema(Cinema cinema);
        Task<Cinema> FindCinemaByName(string cinema);
        Task<Cinema> AddCinemaHall(Cinema cinema, CinemaHall cinemaHall);
        Task<Cinema> RemoveCinemaHall(Cinema cinema, CinemaHall cinemaHall);
        Task<SeatCategory> SaveCategory(SeatCategory category);
        Task<SeatCategory> GetCategoryForCinema(int cinemaId, string name);
        Task<IEnumerable<SeatCategory>> GetCategoriesForCinema(int cinemaId);
        Task<SeatCategory> GetCategory(int id);
        Task<int> RemoveCategory(SeatCategory category);

        Task<int> RemoveCategory(int category);
    }
}
