using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Apollo.Core.Dal.Dao
{
    public class ScheduleDao : ApolloDao<Schedule>, IScheduleDao
    {
        private ICinemaHallDao cinemaHallDao;
        private IMovieDao movieDao;
        private IReservationDao reservationDao;

        public ScheduleDao(IConnectionFactory connectionFactory,
            IQueryBuilderFactory qbFactory,
            ICinemaHallDao cinemaHallDao,
            IMovieDao movieDao,
            IReservationDao reservationDao) : base(connectionFactory, qbFactory)
        {
            this.cinemaHallDao = cinemaHallDao;
            this.movieDao = movieDao;
            this.reservationDao = reservationDao;
        }

        public async Task<IEnumerable<Schedule>> GetScheduleForDay(DateTime date, int cinemaId, int cinemaHallId)
        {
            DateTime midnight = date - date.TimeOfDay;

            var qb = GetQueryBuilder()
                .Table(typeof(Schedule))
                .JoinTable(Column.Create<Schedule>("cinemaHallId"), Column.Create<CinemaHall>("id"))
                .SetCondition(Column.Create<Schedule>("startTime"), OperationType.GreaterEqualThen, midnight)
                .AddAnd(Column.Create<Schedule>("startTime"), OperationType.LessThen, midnight.AddDays(1), "endTime")
                .AddAnd(Column.Create<Schedule>("cinemaHallId"), OperationType.Equals, cinemaHallId)
                .AddAnd(Column.Create<CinemaHall>("cinemaId"), OperationType.Equals, cinemaId);

            IEnumerable<Schedule> schedules = await template.QueryAsync<Schedule>(qb, template.GenericRowMapper<Schedule>);

            foreach (Schedule schedule in schedules)
            {
                ValidateSchedule(schedule);
            }

            return schedules;
        }

        public async Task<IEnumerable<Schedule>> GetScheduleForDay(DateTime date, int cinemaId)
        {
            DateTime midnight = date - date.TimeOfDay;

            var qb = GetQueryBuilder()
                .Table(typeof(Schedule))
                .JoinTable(Column.Create<Schedule>("cinemaHallId"), Column.Create<CinemaHall>("id"))
                .SetCondition(Column.Create<Schedule>("startTime"), OperationType.GreaterEqualThen, midnight)
                .AddAnd(Column.Create<Schedule>("startTime"), OperationType.LessThen, midnight.AddDays(1), "endTime")
                .AddAnd(Column.Create<CinemaHall>("cinemaId"), OperationType.Equals, cinemaId);

            IEnumerable<Schedule> schedules = await template.QueryAsync<Schedule>(qb, template.GenericRowMapper<Schedule>);

            foreach (Schedule schedule in schedules)
            {
                ValidateSchedule(schedule);
                await FillScheduleAsync(schedule);
            }

            return schedules;
        }

        public async Task<int> RemoveSchedulesForCinemaHall(int cinemaHallId) 
        {
            var qb = GetQueryBuilder()
                .Table(typeof(Schedule))
                .SetCondition(Column.Create<Schedule>("cinemaHallId"), OperationType.Equals, cinemaHallId);
            
            var schedules = await template.QueryAsync<Schedule>(qb);

            int affectedRows = 0;

            foreach (Schedule schedule in schedules)
            {
                affectedRows += await DeleteByIdAsync(schedule.Id);
            }

            return affectedRows;
        }

        public async Task<IEnumerable<Schedule>> InsertSchedules(IEnumerable<Schedule> schedules)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                List<Schedule> newSchedules = new List<Schedule>();
                foreach (var schedule in schedules)
                {
                    newSchedules.Add(await CreateAsync(schedule));
                }

                scope.Complete();
                return newSchedules;
            }
        }

        public override async Task<Schedule> CreateAsync(Schedule schedule)
        {
            ValidateSchedule(schedule);

            Schedule newSchedule;

            using (TransactionScope scope = new TransactionScope())
            {
                newSchedule = await base.CreateAsync(schedule);

                ValidateSchedule(newSchedule);

                scope.Complete();
            }

            return newSchedule;
        }

        public override async Task<int> DeleteByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentException("id cannot be < 0");
            }

            int affectedRows = 0;
            
            var qb = GetQueryBuilder()
                .Table(typeof(Reservation))
                .SetCondition(Column.Create<Reservation>("scheduleId"), OperationType.Equals, id);

            var reservations = await template.QueryAsync<Reservation>(qb);
            foreach (Reservation res in reservations)
            {
                affectedRows += await reservationDao.DeleteByIdAsync(res.Id);
            }

            affectedRows += await base.DeleteByIdAsync(id);

            if (affectedRows == 0)
            {
                throw new ArgumentException("Schedule does not exist in DB to be deleted");
            }

            return affectedRows;
        }

        public override async Task<IEnumerable<Schedule>> FindAllAsync()
        {
            IEnumerable<Schedule> schedules = await base.FindAllAsync();

            using (TransactionScope scope = new TransactionScope())
            {
                foreach (var schedule in schedules)
                {
                    ValidateSchedule(schedule);
                }

                scope.Complete();
            }

            return schedules;
        }

        public override async Task<Schedule> FindByIdAsync(int id)
        {
            Schedule schedule = await base.FindByIdAsync(id);
            if (schedule == null)
            {
                return schedule;
            }

            ValidateSchedule(schedule);
            return schedule;
        }

        public override async Task<Schedule> UpdateAsync(Schedule schedule)
        {
            ValidateSchedule(schedule);

            Schedule newSchedule = await base.UpdateAsync(schedule);

            return newSchedule;
        }

        private void ValidateSchedule(Schedule schedule)
        {
            if (schedule.MovieId <= 0)
            {
                throw new ArgumentException("Movie does not exist in DB");
            }

            if (schedule.CinemaHallId <= 0)
            {
                throw new ArgumentException("Cinema does not exist in DB");
            }
        }

        public async Task<IEnumerable<Schedule>> GetScheduleByMovieIdAsync(int movieId, DateTime from, DateTime to)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(Schedule))
                .SetCondition(Column.Create<Schedule>("startTime"), OperationType.LessThanEqual, to, "startTimeTo")
                .AddAnd(Column.Create<Schedule>("startTime"), OperationType.GreaterEqualThen, from, "startTimeFrom")
                .AddAnd(Column.Create<Schedule>("movieId"), OperationType.Equals, movieId);

            return await template.QueryAsync<Schedule>(qb, template.GenericRowMapper<Schedule>);
        }
        private async Task<Schedule> FillScheduleAsync(Schedule schedule)
        {
            schedule.Movie = await movieDao.FindByIdAsync(schedule.MovieId);
            schedule.CinemaHall = await cinemaHallDao.FindByIdAsync(schedule.CinemaHallId);

            return schedule;
        }
    }
}