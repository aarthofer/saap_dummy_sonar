using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Dal.Mock;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.BL.Test
{
    public class ReservationTest
    {
        private MockReservationDao reservationDao = new MockReservationDao();
        private MockUserDao userDao = new MockUserDao();
        private MockScheduleDao scheduleDao = new MockScheduleDao();
        private MockCinemaHallDao cinemaHallDao = new MockCinemaHallDao();
        private MockConfigrationDao configDao = new MockConfigrationDao();
        
        public ReservationTest()
        {

        }

        private IReservationBl GetReservationBl()
        {
            IConfiguration configBl = new ConfigurationBl(configDao.Object);
            IConstraintBl constraintBl = new ConstraintBl();
            return new ReservationBl(reservationDao.Object, userDao.Object, scheduleDao.Object, configBl, constraintBl, cinemaHallDao.Object);
        }

        [Fact]
        public async Task TestAddReservation()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });

            List<CinemaHallSeat> takenSeats = new List<CinemaHallSeat>();
            takenSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 3, Row = 1, cinemaHallId = 1, SeatNr = 3 });

            List<Reservation> takenReservations = new List<Reservation>();
            takenReservations.Add(new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, Seats = takenSeats, UserId = 10 });

            Reservation reservation = new Reservation { Id = -1, IsPayed = false, ScheduleId = 1, Seats = seats, UserId = 1 };

            List<CinemaHallSeat> cinemaHallSeats = new List<CinemaHallSeat>();
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 3, Row = 1, cinemaHallId = 1, SeatNr = 3 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 4, Row = 1, cinemaHallId = 1, SeatNr = 4 });

            CinemaHall hall = new CinemaHall { CinemaId = 1, Id = 1, Name = "Test Hall", Seats = cinemaHallSeats };
            reservationDao
                .MockGetReservations(takenReservations);

            userDao
                .MockFindById(new User { Email = "m@m.m", Username = "Hochpöchler", Password = "1234", Id = 1, Role = User.UserRole.USER });

            scheduleDao
                .MockFindById(new Schedule { Id = 1, MovieId = 1, StartTime = new DateTime(20, 01, 01, 12, 00, 00), CinemaHallId = 1 });

            cinemaHallDao
                .MockGetCinemaHallSeats(cinemaHallSeats);

            IReservationBl reservationBl = GetReservationBl();

            Reservation newReservation = await reservationBl.AddReservationAsync(reservation);

            scheduleDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
            userDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
            reservationDao.Verify(dao => dao.CreateAsync(It.IsAny<Reservation>()), Times.Once);
            cinemaHallDao.VerifyGetSeats(Times.Once);
        }
        [Fact]
        public async Task TestAddReservationAlreadyInDb()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });

            Reservation reservation = new Reservation { Id = 1, IsPayed = false, ScheduleId = 1, Seats = seats, UserId = 1 };

            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.AddReservationAsync(reservation));

            scheduleDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            userDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            reservationDao.Verify(dao => dao.CreateAsync(It.IsAny<Reservation>()), Times.Never);
        }


        [Fact]
        public async Task TestAddReservationInvalidScheduleId()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });

            Reservation reservation = new Reservation { Id = -1, IsPayed = false, ScheduleId = -1, Seats = seats, UserId = 1 };

            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.AddReservationAsync(reservation));

            scheduleDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            userDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            reservationDao.Verify(dao => dao.CreateAsync(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        public async Task TestAddReservationInvalidUserId()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });

            Reservation reservation = new Reservation { Id = -1, IsPayed = false, ScheduleId = 1, Seats = seats, UserId = -1 };

            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.AddReservationAsync(reservation));
            
            scheduleDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            userDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            reservationDao.Verify(dao => dao.CreateAsync(It.IsAny<Reservation>()), Times.Never);
        }


        [Fact]
        public async Task TestAddReservationUserIdNotExistant()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });

            Reservation reservation = new Reservation { Id = -1, IsPayed = false, ScheduleId = 1, Seats = seats, UserId = 1000 };

            reservationDao
                .MockFindById(reservation);

            userDao
                .MockFindById(null);

            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.AddReservationAsync(reservation));

            scheduleDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            userDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
            reservationDao.Verify(dao => dao.CreateAsync(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        public async Task TestAddReservationNoSeats()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();

            Reservation reservation = new Reservation { Id = -1, IsPayed = false, ScheduleId = 1, Seats = seats, UserId = 1000 };

            reservationDao
                .MockFindById(reservation);

            userDao
                .MockFindById(null);

            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.AddReservationAsync(reservation));

            scheduleDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            userDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            reservationDao.Verify(dao => dao.CreateAsync(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        public async Task TestAddReservationSeatAlreadyTaken()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });

            List<CinemaHallSeat> takenSeats = new List<CinemaHallSeat>();
            takenSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });

            Reservation reservation = new Reservation { Id = -1, IsPayed = false, ScheduleId = 1, Seats = seats, UserId = 1 };

            List<Reservation> takenReservations = new List<Reservation>();
            takenReservations.Add(new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, Seats = takenSeats, UserId = 10 });

            List<CinemaHallSeat> cinemaHallSeats = new List<CinemaHallSeat>();
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 3, Row = 1, cinemaHallId = 1, SeatNr = 3 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 4, Row = 1, cinemaHallId = 1, SeatNr = 4 });

            CinemaHall hall = new CinemaHall { CinemaId = 1, Id = 1, Name = "Test Hall", Seats = cinemaHallSeats };

            scheduleDao
                .MockFindById(new Schedule { Id = 1, MovieId = 1, StartTime = new DateTime(20, 01, 01, 12, 00, 00), CinemaHallId = 1});

            cinemaHallDao
                .MockGetCinemaHallSeats(cinemaHallSeats);

            reservationDao.MockGetReservations(takenReservations);
            userDao.MockFindById(new User { Id = 1, Username = "Michael", Email = "m@m.m", Password = "12345", Role = User.UserRole.USER });

            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.AddReservationAsync(reservation));

            scheduleDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
            userDao.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
            cinemaHallDao.VerifyGetSeats(Times.Once);
            reservationDao.Verify(dao => dao.GetReservations(It.IsAny<int>()), Times.Once);
            reservationDao.Verify(dao => dao.CreateAsync(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        public async Task TestGetSeats()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 3, Row = 1, cinemaHallId = 1, SeatNr = 3 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 4, Row = 1, cinemaHallId = 1, SeatNr = 4 });

            List<Reservation> reservations = new List<Reservation>();
            reservations.Add(new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = seats });

            scheduleDao
                .MockFindById(new Schedule { Id = 1 });

            cinemaHallDao
                .MockGetCinemaHallSeats(seats);

            reservationDao
                .MockGetReservations(reservations);

            IReservationBl reservationBl = GetReservationBl();

            IEnumerable<ReservationSeat> reservationSeats = await reservationBl.GetSeatsAsync(1);

            scheduleDao.VerifyFindById(Times.Once);
            cinemaHallDao.VerifyGetSeats(Times.Once);
            reservationDao.VerifyGetReservations(Times.Once);
        }

        [Fact]
        public async Task TestGetSeatsWithConstraints()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Test", Price = 2 };
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            seats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });

            List<CinemaHallSeat> cinemaHallSeats = new List<CinemaHallSeat>();
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 1, cinemaHallId = 1, SeatNr = 1 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 2, Row = 1, cinemaHallId = 1, SeatNr = 2 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 3, Row = 1, cinemaHallId = 1, SeatNr = 3 });
            cinemaHallSeats.Add(new CinemaHallSeat { Id = 1, CategoryId = 1, Col = 1, Row = 2, cinemaHallId = 1, SeatNr = 4 });

            List<Reservation> reservations = new List<Reservation>();
            reservations.Add(new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = seats });

            scheduleDao
                .MockFindById(new Schedule { Id = 1 });

            cinemaHallDao
                .MockGetCinemaHallSeats(seats);

            reservationDao
                .MockGetReservations(reservations);

            configDao
                .MockFindByKeyAsync();

            IReservationBl reservationBl = GetReservationBl();

            IEnumerable<ReservationSeat> reservationSeats = await reservationBl.GetSeatsAsync(1);

            foreach (var seat in reservationSeats)
            {
                if (seat.Seat.Row == 1)
                {
                    Assert.NotEqual(ReservationSeat.State.free, seat.state);
                }
                else
                {
                    Assert.Equal(ReservationSeat.State.free, seat.state);
                }
            }

            scheduleDao.VerifyFindById(Times.Once);
            cinemaHallDao.VerifyGetSeats(Times.Once);
            reservationDao.VerifyGetReservations(Times.Once);
        }

        [Fact]
        public async Task TestGetSeatsScheduleIdNotExists()
        {
            scheduleDao
                .MockFindById(null);

            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.GetSeatsAsync(1));

            scheduleDao.VerifyFindById(Times.Once);
            reservationDao.VerifyGetReservations(Times.Never);
        }

        [Fact]
        public async Task TestGetSeatsScheduleIdInvalid()
        {
            IReservationBl reservationBl = GetReservationBl();

            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.GetSeatsAsync(-1));

            scheduleDao.VerifyFindById(Times.Never);
            reservationDao.VerifyGetReservations(Times.Never);
        }

        [Fact]
        public async Task TestRemoveReservationId()
        {
            reservationDao
                .MockRemoveReservation();

            IReservationBl reservationBl = GetReservationBl();
            await reservationBl.RemoveReservationAsync(1);

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyDeleteById(Times.Once);
        }

        [Fact]
        public async Task TestRemoveReservationIdInvalidId()
        {
            reservationDao
                .MockRemoveReservation();

            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.RemoveReservationAsync(-1));

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyDeleteById(Times.Never);
        }

        [Fact]
        public async Task TestRemoveReservation()
        {
            Reservation reservation = new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            IReservationBl reservationBl = GetReservationBl();
            await reservationBl.RemoveReservationAsync(reservation);

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyDeleteById(Times.Once);
        }

        [Fact]
        public async Task TestRemoveReservationInvalidId()
        {
            Reservation reservation = new Reservation { Id = -1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.RemoveReservationAsync(reservation));

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyDeleteById(Times.Never);
        }

        [Fact]
        public async Task TestRemoveReservationNull()
        {
            Reservation reservation = null;

            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentNullException>(() => reservationBl.RemoveReservationAsync(reservation));

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyDeleteById(Times.Never);
        }

        [Fact]
        public async Task TestPayReservation()
        {
            Reservation reservation = new Reservation { Id = 1, IsPayed = false, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };
            Reservation paidReservation = new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            reservationDao
                .MockUpdateReservation(paidReservation);

            IReservationBl reservationBl = GetReservationBl();
            bool success = await reservationBl.PayReservationAsync(reservation);
            Assert.True(success);

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyUpdate(Times.Once);
        }

        [Fact]
        public async Task TestPayReservationAlreadyPayed()
        {
            Reservation reservation = new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.PayReservationAsync(reservation));

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyUpdate(Times.Never);
        }

        [Fact]
        public async Task TestPayReservationInvalidId()
        {
            Reservation reservation = new Reservation { Id = -1, IsPayed = false, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.PayReservationAsync(reservation));

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyUpdate(Times.Never);
        }

        [Fact]
        public async Task TestPayReservationNull()
        {
            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentNullException>(() => reservationBl.PayReservationAsync(null));

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyUpdate(Times.Never);
        }

        [Fact]
        public async Task TestPayReservationId()
        {
            Reservation unpaidReservation = new Reservation { Id = 1, IsPayed = false, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };
            Reservation paidReservation = new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            reservationDao
                .MockFindById(unpaidReservation)
                .MockUpdateReservation(paidReservation);

            IReservationBl reservationBl = GetReservationBl();
            bool success = await reservationBl.PayReservationAsync(1);

            reservationDao.VerifyFindById(Times.Once);
            reservationDao.VerifyUpdate(Times.Once);
        }

        [Fact]
        public async Task TestPayReservationIdAlreadyPayed()
        {
            Reservation unpaidReservation = new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            reservationDao
                .MockFindById(unpaidReservation);

            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.PayReservationAsync(1));

            reservationDao.VerifyFindById(Times.Once);
            reservationDao.VerifyUpdate(Times.Never);
        }

        [Fact]
        public async Task TestPayReservationIdInvalidId()
        {
            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.PayReservationAsync(-1));

            reservationDao.VerifyFindById(Times.Never);
            reservationDao.VerifyUpdate(Times.Never);
        }

        [Fact]
        public async Task TestPayReservationIdNonExistantId()
        {
            Reservation unpaidReservation = new Reservation { Id = 1, IsPayed = true, ScheduleId = 1, UserId = 1, Seats = new List<CinemaHallSeat>() };

            reservationDao
                .MockFindById(null);

            IReservationBl reservationBl = GetReservationBl();
            await Assert.ThrowsAsync<ArgumentException>(() => reservationBl.PayReservationAsync(1));

            reservationDao.VerifyFindById(Times.Once);
            reservationDao.VerifyUpdate(Times.Never);
        }
    }
}
