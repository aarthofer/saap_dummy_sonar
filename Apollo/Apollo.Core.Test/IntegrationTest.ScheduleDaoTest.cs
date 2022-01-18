using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace Apollo.Core.Test
{
    public class ScheduleDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IPersonDao personDao;
        private IGenreDao genreDao;
        private ICinemaHallDao cinemaHallDao;
        private IMovieDao movieDao;
        private IRoleDao roleDao;
        private IScheduleDao scheduleDao;
        private IReservationDao reservationDao;

        public ScheduleDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            roleDao = new RoleDao(connectionFactory, qbFactory);
            personDao = new PersonDao(connectionFactory, qbFactory, roleDao);
            genreDao = new GenreDao(connectionFactory, qbFactory);
            cinemaHallDao = new CinemaHallDao(connectionFactory, qbFactory);
            movieDao = new MovieDao(connectionFactory, qbFactory, genreDao, personDao);
            reservationDao = new ReservationDao(connectionFactory, qbFactory);
            scheduleDao = new ScheduleDao(connectionFactory, qbFactory, cinemaHallDao, movieDao, reservationDao);
        }

        [Fact]
        public async Task TestScheduleInsert()
        {
            /** TESTS **/
            Movie movie = await movieDao.FindByIdAsync(1);
            CinemaHall cinemaHall = await cinemaHallDao.FindByIdAsync(1);
            DateTime startTime = new DateTime(2010, 12, 01);

            Schedule schedule = new Schedule { CinemaHallId = cinemaHall.Id, MovieId = movie.Id, StartTime = startTime };

            Schedule newSchedule = await scheduleDao.CreateAsync(schedule);

            Assert.Equal(cinemaHall.Id, newSchedule.CinemaHallId);
            Assert.Equal(movie.Id, newSchedule.MovieId);

            Assert.Equal(1, await scheduleDao.DeleteByIdAsync(newSchedule.Id));
        }

        [Fact]
        public async Task TestScheduleUpdate()
        {
            /** TESTS **/
            Movie movie = await movieDao.FindByIdAsync(1);
            CinemaHall cinemaHall = await cinemaHallDao.FindByIdAsync(1);

            DateTime oldStart = new DateTime(year: 2008, month: 12, day: 1, hour: 13, minute: 00, second: 00); ;
            Schedule schedule = await scheduleDao.CreateAsync(new Schedule { CinemaHallId = cinemaHall.Id, MovieId = movie.Id, StartTime = oldStart });

            Assert.NotNull(schedule);
            Assert.Equal(2008, schedule.StartTime.Year);

            schedule.StartTime = new DateTime(year: 2009, month: 12, day: 1, hour: 13, minute: 00, second: 00);

            Schedule updatedSchedule = await scheduleDao.UpdateAsync(schedule);

            Assert.NotNull(updatedSchedule);
            Assert.Equal(2009, schedule.StartTime.Year);

            updatedSchedule.StartTime = oldStart;
            updatedSchedule = await scheduleDao.UpdateAsync(updatedSchedule);
            Assert.Equal(2008, updatedSchedule.StartTime.Year);

            await scheduleDao.DeleteByIdAsync(schedule.Id);
        }

        [Fact]
        public async Task TestInsertMultipleSchedules()
        {
            Movie movie = await movieDao.FindByIdAsync(1);
            CinemaHall cinemaHall = await cinemaHallDao.FindByIdAsync(1);
            DateTime startTime = new DateTime(2010, 12, 01, 8, 00, 00);

            List<Schedule> schedules = new List<Schedule>();
            schedules.Add(new Schedule { CinemaHallId = cinemaHall.Id, MovieId = movie.Id, StartTime = startTime });

            movie = await movieDao.FindByIdAsync(2);
            cinemaHall = await cinemaHallDao.FindByIdAsync(1);
            startTime = new DateTime(2010, 12, 01, 11, 00, 00);

            schedules.Add(new Schedule { CinemaHallId = cinemaHall.Id, MovieId = movie.Id, StartTime = startTime });

            IEnumerable<Schedule> scheduleList = await scheduleDao.InsertSchedules(schedules);
            Assert.NotNull(scheduleList);
            Assert.True(scheduleList.Count() == 2);

            foreach (var schedule in scheduleList)
            {
                await scheduleDao.DeleteByIdAsync(schedule.Id);
                Assert.Null(await scheduleDao.FindByIdAsync(schedule.Id));
            }
        }

        [Fact]
        public async Task TestGetSchedulePerDay()
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                ICinemaDao cinemaDao = new CinemaDao(connectionFactory, qbFactory, cinemaHallDao, scheduleDao);

                Movie movie = await movieDao.FindByIdAsync(1);
                CinemaHall cinemaHall = await cinemaHallDao.FindByIdAsync(1);
                CinemaHall cinemaHall2 = await cinemaHallDao.FindByIdAsync(2);
                DateTime startTime = new DateTime(2010, 12, 01);

                List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
                seats.Add(new CinemaHallSeat { SeatNr = 1, Row = 1, Col = 1, CategoryId = 1 });

                List<CinemaHall> cinemaHalls = new List<CinemaHall>();
                cinemaHalls.Add(new CinemaHall { CinemaId = 2, Name = "Test", Seats = seats });

                Cinema newCinema = await cinemaDao.CreateAsync(new Cinema { Name = "Test", CinemaHalls = cinemaHalls });

                Movie movie1 = await movieDao.FindByIdAsync(1);
                Movie movie2 = await movieDao.FindByIdAsync(2);

                //Create 3 Schedules. 
                //1 For Cinema1 Cinema1.cinemaHall1
                //1 For Cinema1 Cinema1.cinemaHall2
                //1 For Cinema2 Cinema2.cinemaHall1
                List<Schedule> schedules = new List<Schedule>();
                schedules.Add(new Schedule { CinemaHallId = cinemaHall.Id, MovieId = movie1.Id, StartTime = startTime });
                schedules.Add(new Schedule { CinemaHallId = cinemaHall2.Id, MovieId = movie2.Id, StartTime = startTime });
                schedules.Add(new Schedule { CinemaHallId = newCinema.CinemaHalls.First().Id, MovieId = movie2.Id, StartTime = startTime });

                IEnumerable<Schedule> newSchedules = await scheduleDao.InsertSchedules(schedules);

                IEnumerable<Schedule> c1 = await scheduleDao.GetScheduleForDay(startTime, 1);
                IEnumerable<Schedule> c1Hall1 = await scheduleDao.GetScheduleForDay(startTime, 1, 1);

                IEnumerable<Schedule> c2 = await scheduleDao.GetScheduleForDay(startTime, newCinema.Id);

                Assert.Equal(2, c1.Count());
                Assert.Single(c1Hall1);
                Assert.Single(c2);

                foreach (Schedule schedule in newSchedules)
                {
                    await scheduleDao.DeleteByIdAsync(schedule.Id);
                }

                await cinemaDao.RemoveCinema(newCinema);
            }
        }
    }
}