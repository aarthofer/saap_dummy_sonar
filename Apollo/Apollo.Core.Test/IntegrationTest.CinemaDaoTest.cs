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
    public class CinemaDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;

        private ICinemaDao cinemaDao;
        private ICinemaHallDao cinemaHallDao;

        public CinemaDaoTest()
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
        public async Task SimpleCinemaInsertTest()
        {
            Cinema cinema = new Cinema { Name = "Test Cinema" };
            Assert.Equal(-1, cinema.Id);

            Cinema newCinema = await cinemaDao.CreateAsync(cinema);
            Assert.NotEqual(-1, newCinema.Id);

            Assert.Equal(1, await cinemaDao.DeleteByIdAsync(newCinema.Id));

            Assert.Null(await cinemaDao.FindByIdAsync(newCinema.Id));
        }

        [Fact]
        public async Task CinemaUpdateTest()
        {
            string cinemaName = "Test Cinema";
            Cinema cinema = new Cinema { Name = cinemaName };
            Assert.Equal(-1, cinema.Id);

            Cinema newCinema = await cinemaDao.CreateAsync(cinema);
            Assert.NotEqual(-1, newCinema.Id);

            Assert.Equal(cinemaName, (await cinemaDao.FindByIdAsync(newCinema.Id)).Name);

            newCinema.Name = "New Cinema Name";
            await cinemaDao.UpdateAsync(newCinema);

            Assert.NotEqual(cinemaName, (await cinemaDao.FindByIdAsync(newCinema.Id)).Name);

            await cinemaDao.DeleteByIdAsync(newCinema.Id);
            Assert.Null(await cinemaDao.FindByIdAsync(newCinema.Id));
        }

        [Fact]
        public async Task CinemaHallsTest()
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Cinema cinema = await cinemaDao.CreateAsync(new Cinema { Name = "Awesome Cinema" });
                Assert.Empty(cinema.CinemaHalls);

                SeatCategory category = await cinemaDao.SaveCategory(new SeatCategory { Name = "Awesome Category", Price = 8.5, CinemaId = 1 });
                List<CinemaHallSeat> seats = new List<CinemaHallSeat>();
                seats.Add(new CinemaHallSeat { CategoryId = category.Id, SeatNr = 1, Row = 1, Col = 1 });
                seats.Add(new CinemaHallSeat { CategoryId = category.Id, SeatNr = 2, Row = 1, Col = 2 });

                CinemaHall cinemaHall = new CinemaHall { Name = "Saal 1", Seats = seats };

                cinema = await cinemaDao.AddCinemaHall(cinema, cinemaHall);

                Assert.NotEmpty(cinema.CinemaHalls);
                Assert.Single(cinema.CinemaHalls);

                int cinemaHallId = cinema.CinemaHalls.First().Id;

                Assert.Equal("Saal 1", cinema.CinemaHalls.First().Name);
                Assert.Equal(2, cinema.CinemaHalls.First().Seats.Count());

                await cinemaDao.DeleteByIdAsync(cinema.Id);

                Assert.Null(await cinemaHallDao.FindByIdAsync(cinemaHallId));

                await cinemaDao.RemoveCategory(category);
            }
        }

        [Fact]
        public async Task FindCinemaByNameTest()
        {
            Cinema cinema1 = await cinemaDao.CreateAsync(new Cinema { Name = "Awesome Cinema" });
            Cinema cinema2 = await cinemaDao.CreateAsync(new Cinema { Name = "Aweso Cinema" });

            Cinema cinema = await cinemaDao.FindCinemaByName("Aweso Cinema");

            await cinemaDao.DeleteByIdAsync(cinema1.Id);
            await cinemaDao.DeleteByIdAsync(cinema2.Id);

            Assert.NotNull(cinema);
            Assert.Equal(cinema2.Id, cinema.Id);
        }
    }
}