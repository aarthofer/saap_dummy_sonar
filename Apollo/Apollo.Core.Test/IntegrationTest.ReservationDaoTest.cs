using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.Core.Test
{
    public class ReservationDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IReservationDao reservationDao;
        private IUserDao userDao;
        private ICinemaHallDao cinemaDao;

        public ReservationDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            reservationDao = new ReservationDao(connectionFactory, qbFactory);
            userDao = new UserDao(connectionFactory, qbFactory);
            cinemaDao = new CinemaHallDao(connectionFactory, qbFactory);
        }


        [Fact]
        public async Task TestReservationInsert()
        {
            User user = new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            };

            User newUser = await userDao.CreateAsync(user);
            IEnumerable<CinemaHallSeat> seats = await cinemaDao.GetCinemaHallSeats(1);

            IEnumerable<CinemaHallSeat> reserve = seats.Where(seat =>
            {
                if ((seat.Row == 1 || seat.Row == 2) && (seat.Col == 1 || seat.Col == 2 || seat.Col == 4))
                {
                    return true;
                }

                return false;
            });

            Reservation reservation = new Reservation { ScheduleId = 8200, UserId = newUser.Id, IsPayed = false, Seats = reserve };

            Reservation newReservation = await reservationDao.CreateAsync(reservation);

            Assert.NotNull(newReservation);
            Assert.Equal(8200, newReservation.ScheduleId);
            Assert.Equal(6, newReservation.Seats.Count());

            await reservationDao.DeleteByIdAsync(newReservation.Id);
            await userDao.DeleteByIdAsync(newUser.Id);
        }

        [Fact]
        public async Task TestAddToReservation()
        {
            User user = new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            };

            User newUser = await userDao.CreateAsync(user);
            IEnumerable<CinemaHallSeat> seats = await cinemaDao.GetCinemaHallSeats(1);

            IEnumerable<CinemaHallSeat> reserve = seats.Where(seat => seat.Row == 1 && seat.Col == 1);

            Reservation reservation = new Reservation { ScheduleId = 8200, UserId = newUser.Id, IsPayed = false, Seats = reserve };

            Reservation newReservation = await reservationDao.CreateAsync(reservation);

            Assert.NotNull(newReservation);
            Assert.Equal(1, newReservation.Seats.Count());
            Assert.Equal(1, newReservation.Seats.First().Row);
            Assert.Equal(1, newReservation.Seats.First().Col);

            newReservation = await reservationDao.AddSeatsToReservation(newReservation, seats.Where(seat => seat.Row == 1 && seat.Col == 2));

            Assert.NotNull(newReservation);
            Assert.Equal(2, newReservation.Seats.Count());
            Assert.Equal(2, newReservation.Seats.Where(seat => seat.Row == 1 && seat.Col == 2).SingleOrDefault().Col);


            await reservationDao.DeleteByIdAsync(newReservation.Id);
            await userDao.DeleteByIdAsync(newUser.Id);
        }

        [Fact]
        public async Task TestAddSameSeatTwiceToSameReservation()
        {
            User user = new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            };

            User newUser = await userDao.CreateAsync(user);
            IEnumerable<CinemaHallSeat> seats = await cinemaDao.GetCinemaHallSeats(1);

            IEnumerable<CinemaHallSeat> reserve = seats.Where(seat => seat.Row == 1 && seat.Col == 1);

            Reservation reservation = new Reservation { ScheduleId = 8200, UserId = newUser.Id, IsPayed = false, Seats = reserve };

            Reservation newReservation = await reservationDao.CreateAsync(reservation);

            Assert.NotNull(newReservation);
            Assert.Equal(1, newReservation.Seats.Count());
            Assert.Equal(1, newReservation.Seats.First().Row);
            Assert.Equal(1, newReservation.Seats.First().Col);

            await Assert.ThrowsAsync<ArgumentException>(() =>
            {
                return reservationDao.AddSeatsToReservation(newReservation, seats.Where(seat => seat.Row == 1 && seat.Col == 1));
            });

            await reservationDao.DeleteByIdAsync(newReservation.Id);
            await userDao.DeleteByIdAsync(newUser.Id);
        }

        [Fact]
        public async Task TestAddSameSeatTwiceToDifferentReservation()
        {
            User user = new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser1",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            };

            User user2 = new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser2",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            };

            User newUser1 = await userDao.CreateAsync(user);
            User newUser2 = await userDao.CreateAsync(user2);

            IEnumerable<CinemaHallSeat> seats = await cinemaDao.GetCinemaHallSeats(1);

            Reservation reservation = new Reservation
            {
                ScheduleId = 8200,
                UserId = newUser1.Id,
                IsPayed = false,
                Seats = seats.Where(seat => seat.Row == 1 && seat.Col == 1)
            };

            Reservation newReservation = await reservationDao.CreateAsync(reservation);

            reservation = new Reservation
            {
                ScheduleId = 8200,
                UserId = newUser2.Id,
                IsPayed = false,
                Seats = seats.Where(seat => seat.Row == 1 && seat.Col == 1)
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
            {
                return reservationDao.CreateAsync(reservation);
            });

            await reservationDao.DeleteByIdAsync(newReservation.Id);
            await userDao.DeleteByIdAsync(newUser1.Id);
            await userDao.DeleteByIdAsync(newUser2.Id);
        }

        [Fact]
        public async Task TestAddSameSeatTwiceToDifferentSchedule()
        {
            User user = new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser1",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            };

            User user2 = new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser2",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            };

            User newUser1 = await userDao.CreateAsync(user);
            User newUser2 = await userDao.CreateAsync(user2);

            IEnumerable<CinemaHallSeat> seats = await cinemaDao.GetCinemaHallSeats(1);

            Reservation reservation = new Reservation
            {
                ScheduleId = 8100,
                UserId = newUser1.Id,
                IsPayed = false,
                Seats = seats.Where(seat => seat.Row == 1 && seat.Col == 1)
            };

            Reservation newReservation = await reservationDao.CreateAsync(reservation);

            reservation = new Reservation
            {
                ScheduleId = 8200,
                UserId = newUser2.Id,
                IsPayed = false,
                Seats = seats.Where(seat => seat.Row == 1 && seat.Col == 1)
            };

            Reservation newReservation2 = await reservationDao.CreateAsync(reservation);

            Assert.Equal(1, newReservation.Seats.First().Row);
            Assert.Equal(1, newReservation.Seats.First().Col);

            Assert.Equal(1, newReservation2.Seats.First().Row);
            Assert.Equal(1, newReservation2.Seats.First().Col);

            await reservationDao.DeleteByIdAsync(newReservation.Id);
            await reservationDao.DeleteByIdAsync(newReservation2.Id);
            await userDao.DeleteByIdAsync(newUser1.Id);
            await userDao.DeleteByIdAsync(newUser2.Id);
        }

        [Fact]
        public async Task TestRemoveSeatFromReservation()
        {
            User user = await userDao.CreateAsync(new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser1",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            });

            Assert.NotNull(user);

            IEnumerable<CinemaHallSeat> allSeats = await cinemaDao.GetCinemaHallSeats(1);

            Reservation reservation = await reservationDao.CreateAsync(new Reservation
            {
                ScheduleId = 1,
                UserId = user.Id,
                IsPayed = false,
                Seats = allSeats.Where(seat => seat.Row == 1 && seat.Col == 1)
            });

            Assert.Equal(1, reservation.Seats.Count());

            Reservation updatedReservation = await reservationDao.RemoveSeatFromReservation(reservation, allSeats.Where(seat => seat.Row == 1 && seat.Col == 1).SingleOrDefault());

            Assert.Equal(0, updatedReservation.Seats.Count());


            await reservationDao.DeleteByIdAsync(reservation.Id);
            await userDao.DeleteByIdAsync(user.Id);
        }

        [Fact]
        public async Task TestGetReservationsForSchedule()
        {
            User user = await userDao.CreateAsync(new User
            {
                Username = "Ado.ReservationDao.IntegrationTestUser1",
                Password = "1234",
                Role = User.UserRole.USER,
                Email = "m@m.m"
            });

            IEnumerable<CinemaHallSeat> allSeats = await cinemaDao.GetCinemaHallSeats(1);

            var res1 = await reservationDao.CreateAsync(new Reservation
            {
                ScheduleId = 1,
                UserId = user.Id,
                IsPayed = false,
                Seats = allSeats.Where(seat => seat.Row == 1 && seat.Col == 1)
            });

            var res2 = await reservationDao.CreateAsync(new Reservation
            {
                ScheduleId = 1,
                UserId = user.Id,
                IsPayed = false,
                Seats = allSeats.Where(seat => seat.Row == 1 && seat.Col == 4)
            });

            var reservations = await reservationDao.GetReservations(1);
            Assert.Equal(16, reservations.Count());

            foreach (var reservation in reservations)
            {
                Assert.Equal(1, reservation.ScheduleId);
            }

            await reservationDao.DeleteByIdAsync(res1.Id);
            await reservationDao.DeleteByIdAsync(res2.Id);
            await userDao.DeleteByIdAsync(user.Id);
        }
    }
}