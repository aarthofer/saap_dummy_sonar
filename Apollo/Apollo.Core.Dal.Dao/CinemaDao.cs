using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;

namespace Apollo.Core.Dal.Dao
{
    public class CinemaDao : ApolloDao<Cinema>, ICinemaDao
    {
        private ICinemaHallDao cinemaHallDao;
        private IScheduleDao scheduleDao;
        public CinemaDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory, ICinemaHallDao cinemaHallDao, IScheduleDao scheduleDao) : base(connectionFactory, qbFactory)
        {
            this.cinemaHallDao = cinemaHallDao ?? throw new ArgumentNullException(nameof(cinemaHallDao));
            this.scheduleDao = scheduleDao ?? throw new ArgumentNullException(nameof(scheduleDao));
        }

        public override async Task<Cinema> CreateAsync(Cinema cinema)
        {
            Cinema newCinema = await base.CreateAsync(cinema);
            
            if (newCinema == null || newCinema.Id <= 0)
            {
                throw new ArgumentException($"Insert of {nameof(cinema)} into db not possible ");
            }

            List<CinemaHall> newCinemaHalls = new List<CinemaHall>();

            foreach (CinemaHall hall in cinema.CinemaHalls)
            {
                hall.CinemaId = newCinema.Id;
                newCinemaHalls.Add(await cinemaHallDao.CreateAsync(hall));
            }

            newCinema.CinemaHalls = newCinemaHalls;
            return newCinema;
        }

        public override async Task<Cinema> UpdateAsync(Cinema cinema)
        {
            using(TransactionScope scope = new TransactionScope())
            {
                if (cinema == null) 
                {
                    throw new ArgumentNullException("Cinema cannot be null");
                }

                List<int> stillInCinema = new List<int>();

                var updatedCinema = await base.UpdateAsync(cinema);
                foreach (CinemaHall hall in cinema.CinemaHalls)
                {
                    if (hall.Id < 0)
                    {
                        var newHall = await cinemaHallDao.CreateAsync(hall);
                        stillInCinema.Add(newHall.Id);
                    }
                    else 
                    {
                        await cinemaHallDao.UpdateAsync(hall);
                        stillInCinema.Add(hall.Id);
                    }
                }

                await RemoveNotNeededCinemaHalls(cinema.Id, stillInCinema);
                await FillCinema(updatedCinema);
                scope.Complete();
                return updatedCinema;
            }
        }

        private async Task<int> RemoveNotNeededCinemaHalls(int cinemaId, IEnumerable<int> stillInCinema)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(CinemaHall))
                .SetCondition(Column.Create<CinemaHall>("cinemaId"), OperationType.Equals, cinemaId);
            
            var halls = await template.QueryAsync<CinemaHall>(qb);
            int affectedRows = 0;

            foreach (CinemaHall hall in halls)
            {
                if (stillInCinema.Contains(hall.Id)) { continue; }

                await scheduleDao.RemoveSchedulesForCinemaHall(hall.Id);
                affectedRows += await cinemaHallDao.DeleteByIdAsync(hall.Id);
            }

            return affectedRows;
        }

        private async Task<int> RemoveNotNeededCinemaHallSeats(CinemaHall hall)
        {
            var stillUsedSeatIds = hall.Seats.Select(seat => seat.Id);

            var cond = GetQueryBuilder().GetNewCondition();

            var count = 1;
            foreach (var seat in hall.Seats)
            {
                cond.AddOr(Column.Create<CinemaHallSeat>("cinemaHallId"), OperationType.NotEquals, seat.Id, $"cinemaHallIdDelete{count++}");
            }

            var qb = GetQueryBuilder()
                .Table(typeof(CinemaHallSeat))
                .QueryType(QueryTypeEnum.DELETE)
                .SetCondition(Column.Create<CinemaHallSeat>("cinemaHallId"), OperationType.Equals, hall.Id)
                .AddAnd(cond);

            return await template.ExecuteAsync(qb);
        }

        public override async Task<IEnumerable<Cinema>> FindAllAsync()
        {
            IEnumerable<Cinema> cinemas = await base.FindAllAsync();
            foreach (var cinema in cinemas)
            {
                await FillCinema(cinema);
            }

            return cinemas;
        }

        public override async Task<Cinema> FindByIdAsync(int id)
        {
            Cinema cinema = await base.FindByIdAsync(id);
            await FillCinema(cinema);
            return cinema;
        }

        public async Task<Cinema> FindCinemaByName(string name)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(Cinema))
                .SetCondition(Column.Create<Cinema>("name"), OperationType.Equals, name);

            Cinema cinema = await template.QuerySingleAsync<Cinema>(qb);

            await FillCinema(cinema);

            return cinema;
        }

        public override async Task<int> DeleteByIdAsync(int id)
        {
            Cinema cinema = await FindByIdAsync(id);
            return await RemoveCinema(cinema);
        }

        public async Task<int> RemoveCinema(Cinema cinema)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                foreach (CinemaHall hall in cinema.CinemaHalls)
                {
                    await cinemaHallDao.DeleteCinemaHallAsync(hall);
                }

                int affectedRows = await base.DeleteByIdAsync(cinema.Id); 
                scope.Complete();
                return affectedRows;
            }
        }

        public async Task<SeatCategory> GetCategoryForCinema(int cinemaId, string name)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(SeatCategory))
                .SetCondition(Column.Create<SeatCategory>("name"), OperationType.Like, name)
                .AddAnd(Column.Create<SeatCategory>("cinemaId"), OperationType.Equals, cinemaId)
                .Limit(1);

            return await template.QuerySingleAsync<SeatCategory>(qb);
        }

        public async Task<IEnumerable<SeatCategory>> GetCategoriesForCinema(int cinemaId)
        {
            return await base.FindByColumn<SeatCategory>("cinemaId", cinemaId);
        }

        public async Task<SeatCategory> GetCategory(int id)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(SeatCategory))
                .SetCondition(Column.Create<SeatCategory>("id"), OperationType.Like, id)
                .Limit(1);

            return await template.QuerySingleAsync<SeatCategory>(qb);
        }

        public async Task<SeatCategory> SaveCategory(SeatCategory category)
        {
            if (category.Id <= 0)
            {
                return await CreateAsync<SeatCategory>(category);
            }
            else
            {
                return await UpdateAsync<SeatCategory>(category);
            }
        }

        public async Task<int> RemoveCategory(SeatCategory category)
        {
            return await DeleteByIdAsync<SeatCategory>(category.Id);
        }

        public async Task<Cinema> AddCinemaHall(Cinema cinema, CinemaHall cinemaHall)
        {
            cinemaHall.CinemaId = cinema.Id;
            await cinemaHallDao.CreateAsync(cinemaHall);
            return await FindByIdAsync(cinema.Id);
        }

        public async Task<Cinema> RemoveCinemaHall(Cinema cinema, CinemaHall cinemaHall)
        {
            if (cinema.Id != cinemaHall.CinemaId)
            {
                throw new ArgumentException("Cinema id in cinema and cinemaHall doesn't match");
            }

            await DeleteByIdAsync<CinemaHall>(cinemaHall.Id);
            return await FindByIdAsync(cinema.Id);
        }

        private async Task FillCinema(Cinema cinema)
        {
            if (cinema == null) { return;  }
            cinema.CinemaHalls = await cinemaHallDao.FindCinemaHallsByCinemaId(cinema.Id);
            cinema.Categories = await FindByColumn<SeatCategory>("cinemaId", cinema.Id);
        }

        public async Task<int> RemoveCategory(int categoryId)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                var result = await DeleteByIdAsync<SeatCategory>(categoryId);
                scope.Complete();
                return result;
            }
        }
    }
}
