using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class CinemaBl : ICinemaBl
    {
        private ICinemaDao cinemaDao;
        private ICinemaHallDao cinemaHallDao;

        public CinemaBl(ICinemaDao cinemaDao, ICinemaHallDao cinemaHallDao)
        {
            this.cinemaDao = cinemaDao;
            this.cinemaHallDao = cinemaHallDao;
        }

        public async Task<Cinema> GetCinemaByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentException("Id for cinema has to be >= 0");
            }

            return await cinemaDao.FindByIdAsync(id);
        }

        public async Task<Cinema> CreateCinemaAsync(Cinema cinema)
        {
            if (cinema == null)
            {
                throw new ArgumentNullException(nameof(cinema));
            }

            Cinema cinemaInDb = await cinemaDao.FindCinemaByName(cinema.Name);
            if (cinemaInDb != null)
            {
                throw new ArgumentException($"Cinema with name {cinema.Name} already exists");
            }

            return await cinemaDao.CreateAsync(cinema);
        }

        public async Task<int> DeleteCinemaAsync(Cinema cinema)
        {
            if (cinema == null)
            {
                throw new ArgumentNullException(nameof(cinema));
            }

            if (cinema.Id < 0)
            {
                throw new ArgumentException("Cinema not in database if id < 0");
            }

            return await cinemaDao.RemoveCinema(cinema);
        }

        public async Task<int> DeleteCinemaAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentException("Id has to be >= 0");
            }

            Cinema cinema = await cinemaDao.FindByIdAsync(id);

            if (cinema == null)
            {
                throw new ArgumentException($"No cinema found with id {id}");
            }

            return await cinemaDao.RemoveCinema(cinema);
        }

        public async Task<Cinema> UpdateCinemaAsync(Cinema cinema)
        {
            if (cinema == null)
            {
                throw new ArgumentNullException("Cinema cannot be null");
            }

            if (cinema.Id < 0)
            {
                throw new ArgumentException("Cinema Id has to be >= 0");
            }

            return await cinemaDao.UpdateAsync(cinema);
        }

        public async Task<SeatCategory> SaveCategoryAsync(SeatCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            SeatCategory CategoryInDB = await GetCategoryForCinemaByNameAsync(category.CinemaId, category.Name);

            if (CategoryInDB != null && CategoryInDB.Price.CompareTo(category.Price) == 0)
            {
                return CategoryInDB;
            }

            return await cinemaDao.SaveCategory(category);
        }
        public async Task<int> DeleteCategoryAsync(SeatCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException("Category cannot be null");
            }
            if (category.Id < 0)
            {
                throw new ArgumentException("Category Id has to be >= 0");
            }

            return await cinemaDao.RemoveCategory(category);
        }

        public async Task<int> DeleteCategoryByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentException("Category Id has to be >= 0");
            }

            return await cinemaDao.RemoveCategory(id);
        }

        public async Task<SeatCategory> GetCategoryByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentException("Category Id has to be >= 0");
            }

            return await cinemaDao.GetCategory(id);
        }

        public async Task<IEnumerable<SeatCategory>> GetCategoriesForCinemaAsync(int cinemaId)
        {
            if (cinemaId < 0)
            {
                throw new ArgumentException("Cinema Id has to be >= 0");
            }

            return await cinemaDao.GetCategoriesForCinema(cinemaId);
        }

        public async Task<SeatCategory> GetCategoryForCinemaByNameAsync(int cinemaId, string name)
        {
            if (cinemaId < 0)
            {
                throw new ArgumentException("Cinema Id has to be >= 0");
            }

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Cinema name cannot be null or empty");
            }

            return await cinemaDao.GetCategoryForCinema(cinemaId, name);
        }

        public async Task<CinemaHallSeat> SaveCinemaHallSeat(CinemaHallSeat seat)
        {
            return await cinemaHallDao.SaveCinemaHallSeat(seat);
        }
        public async Task<IEnumerable<CinemaHallSeat>> GetCinemaHallSeats(int cinemaHallId)
        {
            return await cinemaHallDao.GetCinemaHallSeats(cinemaHallId);
        }

        public async Task<CinemaHall> GetCinemaHall(int cinemaHallId)
        {
            if (cinemaHallId < 0)
            {
                throw new ArgumentException("Cinema Id has to be >= 0");
            }

            return await cinemaHallDao.FindByIdAsync(cinemaHallId);
        }
        public async Task<IEnumerable<CinemaHallSeat>> ResetAllSeats(int cinemaHallId, IEnumerable<CinemaHallSeat> seats)
        {
            if (cinemaHallId < 0)
            {
                throw new ArgumentException("CinemaHall Id has to be >= 0");
            }

            if (seats == null || seats.Count() == 0)
            {
                throw new ArgumentException("No Seats specified");
            }

            return await cinemaHallDao.ResetAllSeats(cinemaHallId, seats);
        }

    }
}
