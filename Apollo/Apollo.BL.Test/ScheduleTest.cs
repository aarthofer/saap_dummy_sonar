using Apollo.BLInterface;
using Apollo.Core.Dal.Mock;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.BL.Test
{
    public class ScheduleTest
    {
        private MockScheduleDao scheduleDao = new MockScheduleDao();
        private MockCinemaHallDao cinemaHallDao = new MockCinemaHallDao();
        private MockMovieDao movieDao = new MockMovieDao();

        public ScheduleTest()
        {
            movieDao.Init();
        }

        [Fact]
        public async Task TestInsertSchedule()
        {
            SeatCategory cat = new SeatCategory { Id = 1, CinemaId = 1, Name = "Cat", Price = 299 };
            Movie movie = new Movie { };

            List <CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 1, SeatNr = 1, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 2, Row = 1, Col = 2, SeatNr = 2, CategoryId = 1, cinemaHallId = 1 });

            CinemaHall testHall = new CinemaHall { Id = 1, CinemaId = 1, Name = "Test Hall", Seats = seats };

            cinemaHallDao
                .MockFindById(testHall);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            DateTime start = new DateTime(2020, 01, 01, 12, 00, 00);

            Schedule schedule = new Schedule { CinemaHallId = 1, MovieId = 1, StartTime = start };

            Schedule insertedSchedule = await scheduleBl.CreateScheduleAsync(1, schedule);

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Once);
            scheduleDao.VerifyCreateSchedule(Times.Once);
            cinemaHallDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestInsertScheduleNull()
        {
            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await  Assert.ThrowsAsync<ArgumentNullException>(() => scheduleBl.CreateScheduleAsync(1, null));

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Never);
            scheduleDao.VerifyCreateSchedule(Times.Never);
            cinemaHallDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestInsertScheduleInvalidMovieId()
        {
            SeatCategory cat = new SeatCategory { Id = 1, CinemaId = 1, Name = "Cat", Price = 299 };
            Movie movie = new Movie { Id = -1 };

            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 1, SeatNr = 1, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 2, Row = 1, Col = 2, SeatNr = 2, CategoryId = 1, cinemaHallId = 1 });

            CinemaHall testHall = new CinemaHall { Id = 1, CinemaId = 1, Name = "Test Hall", Seats = seats };

            cinemaHallDao
                .MockFindById(testHall);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            DateTime start = new DateTime(2020, 01, 01, 12, 00, 00);

            Schedule schedule = new Schedule { CinemaHallId = 1, MovieId = -1, StartTime = start };

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.CreateScheduleAsync(1, schedule));

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Never);
            scheduleDao.VerifyCreateSchedule(Times.Never);
            cinemaHallDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestInsertScheduleInvalidCinemaHallId()
        {
            SeatCategory cat = new SeatCategory { Id = 1, CinemaId = 1, Name = "Cat", Price = 299 };
            Movie movie = new Movie { Id = 1 };

            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 1, SeatNr = 1, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 2, Row = 1, Col = 2, SeatNr = 2, CategoryId = 1, cinemaHallId = 1 });

            CinemaHall testHall = new CinemaHall { Id = -1, CinemaId = 1, Name = "Test Hall", Seats = seats };

            cinemaHallDao
                .MockFindById(testHall);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            DateTime start = new DateTime(2020, 01, 01, 12, 00, 00);

            Schedule schedule = new Schedule { CinemaHallId = -1, MovieId = 1, StartTime = start };

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.CreateScheduleAsync(1, schedule));

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Never);
            scheduleDao.VerifyCreateSchedule(Times.Never);
            cinemaHallDao.VerifyFindById(Times.Never);
        }


        [Fact]
        public async Task TestInsertScheduleMovieStartConflict()
        {
            SeatCategory cat = new SeatCategory { Id = 1, CinemaId = 1, Name = "Cat", Price = 299 };
            Movie movie = new Movie
            {
                Id = 1,
                Title = "Movie 1",
                Description = "Description 1",
                DurationMinutes = 60,
                Genre = null,
                Image = "Image",
                ReleaseDate = new DateTime(2020, 10, 10),
                Actors = null
            };

            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 1, SeatNr = 1, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 2, Row = 1, Col = 2, SeatNr = 2, CategoryId = 1, cinemaHallId = 1 });

            CinemaHall testHall = new CinemaHall { Id = 1, CinemaId = 1, Name = "Test Hall", Seats = seats };

            List<Schedule> scheduleOfDay = new List<Schedule>();
            scheduleOfDay.Add(new Schedule { Id = 1, CinemaHallId = 1, MovieId = 1, StartTime = new DateTime(2020, 01, 01, 12, 00, 00) });
            scheduleOfDay.Add(new Schedule { Id = 1, CinemaHallId = 1, MovieId = 1, StartTime = new DateTime(2020, 01, 01, 13, 30, 00) });

            scheduleDao.MockGetScheduleForCinemaHallForDay(scheduleOfDay);

            cinemaHallDao
                .MockFindById(testHall);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            DateTime start = new DateTime(2020, 01, 01, 12, 00, 00);

            Schedule schedule = new Schedule { CinemaHallId = 1, MovieId = 1, StartTime = start };

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.CreateScheduleAsync(1, schedule));

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Once);
            scheduleDao.VerifyCreateSchedule(Times.Never);
            cinemaHallDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestInsertScheduleMovieEndConflict()
        {
            SeatCategory cat = new SeatCategory { Id = 1, CinemaId = 1, Name = "Cat", Price = 299 };
            Movie movie = new Movie
            {
                Id = 1,
                Title = "Movie 1",
                Description = "Description 1",
                DurationMinutes = 60,
                Genre = null,
                Image = "Image",
                ReleaseDate = new DateTime(2020, 10, 10),
                Actors = null
            };

            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 1, SeatNr = 1, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 2, Row = 1, Col = 2, SeatNr = 2, CategoryId = 1, cinemaHallId = 1 });

            CinemaHall testHall = new CinemaHall { Id = 1, CinemaId = 1, Name = "Test Hall", Seats = seats };

            List<Schedule> scheduleOfDay = new List<Schedule>();
            scheduleOfDay.Add(new Schedule { Id = 1, CinemaHallId = 1, MovieId = 1, StartTime = new DateTime(2020, 01, 01, 12, 00, 00) });
            scheduleOfDay.Add(new Schedule { Id = 1, CinemaHallId = 1, MovieId = 1, StartTime = new DateTime(2020, 01, 01, 13, 30, 00) });

            scheduleDao.MockGetScheduleForCinemaHallForDay(scheduleOfDay);

            cinemaHallDao
                .MockGetCinemaHallSeats(seats);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            DateTime start = new DateTime(2020, 01, 01, 11, 30, 00);

            Schedule schedule = new Schedule { CinemaHallId = 1, MovieId = 1, StartTime = start };

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.CreateScheduleAsync(1, schedule));

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Once);
            scheduleDao.VerifyCreateSchedule(Times.Never);
            cinemaHallDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestDeleteScheduleId()
        {
            scheduleDao
                .MockDelete();

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await scheduleBl.DeleteSchedule(1);

            scheduleDao.VerifyDeleteSchedule(Times.Once);
            scheduleDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestDeleteScheduleIdInvalidId()
        {
            scheduleDao
                .MockDelete();

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.DeleteSchedule(-11));

            scheduleDao.VerifyDeleteSchedule(Times.Never);
            scheduleDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestDeleteSchedule()
        {
            scheduleDao
                .MockDelete();

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await scheduleBl.DeleteSchedule(new Schedule { Id = 1 });

            scheduleDao.VerifyDeleteSchedule(Times.Once);
            scheduleDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestDeleteScheduleInvalidId()
        {
            scheduleDao
                .MockDelete();

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.DeleteSchedule(new Schedule { Id = -11 }));

            scheduleDao.VerifyDeleteSchedule(Times.Never);
            scheduleDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestDeleteScheduleNull()
        {
            scheduleDao
                .MockDelete();

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => scheduleBl.DeleteSchedule(null));

            scheduleDao.VerifyDeleteSchedule(Times.Never);
            scheduleDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestGetScheduleOfDay()
        {
            IEnumerable<Schedule> schedules = GetMockSchedules();

            scheduleDao
                .MockGetScheduleForCinemaForDay(schedules);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            IEnumerable<Schedule> daySchedules = await scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), 1);

            scheduleDao.VerifyGetScheduleForCinemaForDay(Times.Once);
        }

        [Fact]
        public async Task TestGetScheduleOfDayNull()
        {
            scheduleDao
                .MockGetScheduleForCinemaForDay(null);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            IEnumerable<Schedule> daySchedules = await scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), 1);

            scheduleDao.VerifyGetScheduleForCinemaForDay(Times.Once);
        }

        [Fact]
        public async Task TestGetScheduleOfDayInvalidId()
        {
            scheduleDao
                .MockGetScheduleForCinemaForDay(null);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentException>( () => scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), -1));

            scheduleDao.VerifyGetScheduleForCinemaForDay(Times.Never);
        }

        [Fact]
        public async Task TestGetScheduleOfCinemaHallOfDay()
        {
            IEnumerable<Schedule> schedules = GetMockSchedules();

            scheduleDao
                .MockGetScheduleForCinemaHallForDay(schedules);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            IEnumerable<Schedule> daySchedules = await scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), 1, 1);

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Once);
        }

        [Fact]
        public async Task TestGetScheduleOfCinemaHallOfDayNull()
        {
            scheduleDao
                .MockGetScheduleForCinemaHallForDay(null);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            IEnumerable<Schedule> daySchedules = await scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), 1, 1);

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Once);
        }

        [Fact]
        public async Task TestGetScheduleOfCinemaHallOfDayInvalidId()
        {
            scheduleDao
                .MockGetScheduleForCinemaHallForDay(null);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), -1, 1));

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Never);
        }

        [Fact]
        public async Task TestGetScheduleOfCinemaHallOfDayInvalidCinemaHallId()
        {
            scheduleDao
                .MockGetScheduleForCinemaHallForDay(null);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), 1, -1));

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Never);
        }

        [Fact]
        public async Task TestGetScheduleOfCinemaHallOfDayNoSchedule()
        {
            scheduleDao
                .MockGetScheduleForCinemaHallForDay(null);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await scheduleBl.GetSchedulesOfDay(new DateTime(2020, 01, 01), 1, 1);

            scheduleDao.VerifyGetScheduleForCinemaHallForDay(Times.Once);
        }

        [Fact]
        public async Task TestGetScheduleById()
        {
            IEnumerable<Schedule> schedules = GetMockSchedules();

            scheduleDao
                .MockFindById(schedules.First());

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            Schedule schedule = await scheduleBl.GetScheduleByIdAsync(1);

            scheduleDao.VerifyFindById(Times.Once);
        }

        [Fact]
        public async Task TestGetScheduleByIdInvalid()
        {
            IEnumerable<Schedule> schedules = GetMockSchedules();

            scheduleDao
                .MockFindById(schedules.First());

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.GetScheduleByIdAsync(-11));

            scheduleDao.VerifyFindById(Times.Never);
        }

        [Fact]
        public async Task TestUpdateSchedule()
        {
             List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 1, SeatNr = 1, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 2, SeatNr = 2, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 4, SeatNr = 3, CategoryId = 1, cinemaHallId = 1 });
            
            Schedule schedule = GetMockSchedules().First();
            CinemaHall cinemaHall = new CinemaHall { Id = 1, Name = "Test", CinemaId = 1, Seats = seats };

            scheduleDao
                .MockUpdate(schedule);
            
            cinemaHallDao
                .MockFindById(cinemaHall);

            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await scheduleBl.UpdateSchedule(schedule);

            scheduleDao.VerifyUpdateSchedule(Times.Once);
        }

        [Fact]
        public async Task TestUpdateScheduleNull()
        {
            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => scheduleBl.UpdateSchedule(null));

            scheduleDao.VerifyUpdateSchedule(Times.Never);
        }

        [Fact]
        public async Task TestUpdateScheduleInvalidId()
        {
            Schedule schedule = GetMockSchedules().First();
            schedule.Id = -1;
         
            IScheduleBl scheduleBl = new ScheduleBl(scheduleDao.Object, cinemaHallDao.Object, movieDao.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => scheduleBl.UpdateSchedule(schedule));

            scheduleDao.VerifyUpdateSchedule(Times.Never);
        }

        private IEnumerable<Schedule> GetMockSchedules()
        {
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 1, SeatNr = 1, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 2, SeatNr = 2, CategoryId = 1, cinemaHallId = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, Row = 1, Col = 4, SeatNr = 3, CategoryId = 1, cinemaHallId = 1 });

            CinemaHall cinemaHall = new CinemaHall { Id = 1, Name = "Test", CinemaId = 1, Seats = seats };

            Movie movie = new Movie
            {
                Id = 1,
                Title = "Movie 1",
                Description = "Description 1",
                DurationMinutes = 60,
                Genre = new List<Genre>(),
                Image = "Image",
                ReleaseDate = new DateTime(2020, 10, 10),
                Actors = new List<Person>()
            };

            List<Schedule> schedules = new List<Schedule>();
            schedules.Add(new Schedule { Id = 1, CinemaHallId = 1, MovieId = 1, StartTime = new DateTime(2020, 1, 1, 12, 00, 00) });
            schedules.Add(new Schedule { Id = 1, CinemaHallId = 1, MovieId = 1, StartTime = new DateTime(2020, 1, 1, 13, 15, 00) });
            schedules.Add(new Schedule { Id = 1, CinemaHallId = 1, MovieId = 1, StartTime = new DateTime(2020, 1, 1, 14, 30, 00) });

            return schedules;
        }
    }
}
