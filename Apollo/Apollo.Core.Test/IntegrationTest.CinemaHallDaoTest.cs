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
    public class CinemaHallDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;

        private ICinemaDao cinemaDao;
        private ICinemaHallDao cinemaHallDao;

        public CinemaHallDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            var roleDao = new RoleDao(connectionFactory, qbFactory);
            var genreDao = new GenreDao(connectionFactory, qbFactory);
            var personDao = new PersonDao(connectionFactory, qbFactory, roleDao);
            var reservationDao = new ReservationDao(connectionFactory, qbFactory);
            var movieDao = new MovieDao(connectionFactory, qbFactory, genreDao, personDao);
            var scheduleDao = new ScheduleDao(connectionFactory, qbFactory, cinemaHallDao, movieDao, reservationDao);

            cinemaHallDao = new CinemaHallDao(connectionFactory, qbFactory);
            cinemaDao = new CinemaDao(connectionFactory, qbFactory, cinemaHallDao, scheduleDao);
        }

        [Fact]
        public async Task TestSimpleCinemaInsert()
        {
            /** TESTS **/
            CinemaHall cinemaHall = new CinemaHall { Name= "Test Cinema", CinemaId= 1, Seats = new List<CinemaHallSeat>()};
            var task1 = await cinemaHallDao.FindAllAsync();

            CinemaHall newCinema = await cinemaHallDao.CreateAsync(cinemaHall);

            var task2 = await cinemaHallDao.FindAllAsync();

            Assert.NotNull(task1);
            Assert.NotNull(task2);

            Assert.True(task1.Count() < task2.Count());

            await cinemaHallDao.DeleteCinemaHallAsync(newCinema);
        }

        [Fact]
        public async Task TestCinemaInsert()
        {
            /** TESTS **/
            SeatCategory category1 = await cinemaDao.SaveCategory(new SeatCategory { Name = "Luxury suite", Price = 199.99, CinemaId = 1 });
            SeatCategory category2 = await cinemaDao.SaveCategory(new SeatCategory { Name = "Third class", Price = 2.99, CinemaId = 1 });

            Assert.NotNull(category1);
            Assert.NotNull(category2);

            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
            seats.Add(new CinemaHallSeat { Row = 1, Col = 1, SeatNr = 2, CategoryId = category1.Id });
            seats.Add(new CinemaHallSeat { Row = 1, Col = 2, SeatNr = 3, CategoryId = category2.Id });

            CinemaHall cinema = new CinemaHall { Name = "TestCinema", CinemaId = 1, Seats = seats };

            var cinemasBefore = await cinemaHallDao.FindAllAsync();
            CinemaHall newCinema = await cinemaHallDao.CreateAsync(cinema);

            var cinemasAfter = await cinemaHallDao.FindAllAsync();

            Assert.True(cinemasBefore.Count() < cinemasAfter.Count());

            //Check each Property is filled
            Assert.True(newCinema.Name.Equals("TestCinema"));
            Assert.True(newCinema.Seats.Count() == 2);

            foreach (var seat in newCinema.Seats)
            {
                Assert.Equal(1, seat.Row);
            }

            await cinemaHallDao.DeleteCinemaHallAsync(newCinema);
            await cinemaDao.RemoveCategory(category1);
            await cinemaDao.RemoveCategory(category2);
        }

        [Fact]
        public async Task TestCinemaUpdate()
        {
            /** TESTS **/
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();

            SeatCategory category = await cinemaDao.SaveCategory(new SeatCategory { Name = "TestCategory", Price = 199.99, CinemaId = 1 });

            Assert.NotNull(category);

            seats.Add(new CinemaHallSeat { Row = 2, Col = 20, SeatNr = 1, CategoryId = category.Id });

            CinemaHall cinema = new CinemaHall { Name = "CategoryTestCinema", CinemaId = 1, Seats = seats };

            CinemaHall newCinema = await cinemaHallDao.CreateAsync(cinema);

            Assert.Equal(category.Id, newCinema.Seats.First().CategoryId);
            Assert.Equal(20, newCinema.Seats.First().Col);

            newCinema.Seats.First().Col = 3;
            newCinema.Name = "Best Cinema in town";

            CinemaHall updatedCinema = await cinemaHallDao.UpdateAsync(newCinema);

            Assert.Equal(3, updatedCinema.Seats.First().Col);
            Assert.Equal("Best Cinema in town", updatedCinema.Name);

            await cinemaHallDao.DeleteCinemaHallAsync(updatedCinema);
            await cinemaDao.RemoveCategory(category);
        }

        [Fact]
        public async Task TestCinemaDelete()
        {
            /** TESTS **/
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();

            SeatCategory category = await cinemaDao.SaveCategory(new SeatCategory { Name = "DeleteCategory", Price = 9.80, CinemaId = 1 });

            Assert.NotNull(category);

            seats.Add(new CinemaHallSeat { Row = 3, Col = 3, SeatNr = 1, CategoryId = category.Id });

            CinemaHall cinema = new CinemaHall { Name = "CategoryTestCinema", CinemaId = 1, Seats = seats };

            CinemaHall newCinema = await cinemaHallDao.CreateAsync(cinema);

            int deletedRows = await cinemaHallDao.DeleteByIdAsync(newCinema.Id);

            CinemaHall checkCinema = await cinemaHallDao.FindByIdAsync(newCinema.Id);

            Assert.Null(checkCinema);
            Assert.Equal(2, deletedRows);

            await cinemaDao.RemoveCategory(category);
        }

        [Fact]
        public async Task TestInsertSameSeatTwice()
        {
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();

            SeatCategory category = await cinemaDao.SaveCategory(new SeatCategory { Name = "DeleteCategory", Price = 9.80, CinemaId = 1 });

            Assert.NotNull(category);

            seats.Add(new CinemaHallSeat { Row = 3, Col = 3, SeatNr = 1, CategoryId = category.Id });
            seats.Add(new CinemaHallSeat { Row = 3, Col = 3, SeatNr = 2, CategoryId = category.Id });

            CinemaHall cinema = new CinemaHall { Name = "CategoryTestCinema", CinemaId = 1, Seats = seats };

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaHallDao.CreateAsync(cinema));
            await cinemaDao.RemoveCategory(category);
        }

        [Fact]
        public async Task TestInsertSameCinemaTwice()
        {
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();

            SeatCategory category = await cinemaDao.SaveCategory(new SeatCategory { Name = "DeleteCategory", Price = 9.80, CinemaId = 1 });

            Assert.NotNull(category);

            seats.Add(new CinemaHallSeat { Row = 3, Col = 3, SeatNr = 1, CategoryId = category.Id });

            CinemaHall cinema = new CinemaHall { Name = "CategoryTestCinema", CinemaId = 1, Seats = seats };

            CinemaHall newCinema = await cinemaHallDao.CreateAsync(cinema);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaHallDao.CreateAsync(cinema));

            await cinemaHallDao.DeleteByIdAsync(newCinema.Id);
            await cinemaDao.RemoveCategory(category);
        }

        [Fact]
        public async Task TestInsertSeatWithSameSeatNrTwice()
        {
            List<CinemaHallSeat> seats = new List<CinemaHallSeat>();

            SeatCategory category = await cinemaDao.SaveCategory(new SeatCategory { Name = "DeleteCategory", Price = 9.80, CinemaId = 1 });

            Assert.NotNull(category);

            seats.Add(new CinemaHallSeat { Row = 3, Col = 3, SeatNr = 1, CategoryId = category.Id });
            seats.Add(new CinemaHallSeat { Row = 4, Col = 4, SeatNr = 1, CategoryId = category.Id });

            CinemaHall cinema = new CinemaHall { Name = "CategoryTestCinema", CinemaId = 1, Seats = seats };

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaHallDao.CreateAsync(cinema));

            await cinemaDao.RemoveCategory(category);
        }

        [Fact]
        public async Task TestGetCinemaHallsByCinemaId()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                List<CinemaHallSeat> seats = new List<CinemaHallSeat>();

                Cinema newCinema = await cinemaDao.CreateAsync(new Cinema { Name = "Test Cinema", CinemaHalls = new List<CinemaHall>() });
                SeatCategory category = await cinemaDao.SaveCategory(new SeatCategory { Name = "DeleteCategory", Price = 9.80, CinemaId = newCinema.Id });

                Assert.NotNull(category);

                seats.Add(new CinemaHallSeat { Row = 3, Col = 3, SeatNr = 1, CategoryId = category.Id });
                seats.Add(new CinemaHallSeat { Row = 4, Col = 4, SeatNr = 2, CategoryId = category.Id });

                List<CinemaHall> cinemaHalls = new List<CinemaHall>();

                await cinemaDao.AddCinemaHall(newCinema, new CinemaHall { Name = "CategoryTestCinema1", CinemaId = newCinema.Id, Seats = seats });
                await cinemaDao.AddCinemaHall(newCinema, new CinemaHall { Name = "CategoryTestCinema2", CinemaId = newCinema.Id, Seats = seats });

                IEnumerable<CinemaHall> halls = await cinemaHallDao.FindCinemaHallsByCinemaId(newCinema.Id);

                Assert.Equal(2, halls.Count());

                foreach (CinemaHall hall in halls)
                {
                    Assert.Equal(2, hall.Seats.Count());
                }

            }
        }
    }
}