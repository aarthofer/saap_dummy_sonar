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
    public class GenreDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IGenreDao genreDao;
        private IMovieDao movieDao;

        public GenreDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            genreDao = new GenreDao(connectionFactory, qbFactory);
            movieDao = new MovieDao(connectionFactory, qbFactory, genreDao, new PersonDao(connectionFactory, qbFactory, new RoleDao(connectionFactory, qbFactory)));
        }

        [Theory]
        [InlineData("Komödie", 3)]
        [InlineData("TestGenre", -1)]
        [InlineData("Science Fiction", 12)]
        [InlineData("science fiction", 12)]
        public async Task CreateGenreTest(string name, int id)
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Genre g = await genreDao.GetOrAddGenreByNameAsync(name);
                if (id > 0)
                {
                    Assert.Equal(id, g.Id);
                }
                else
                {
                    Assert.True(g.Id > 18);
                }
            } // do not commit transacionScope
        }

        [Theory]
        [InlineData("Komödie", 1)]
        [InlineData("fa", 2)]
        [InlineData("invalidGenre", 0)]
        public async Task FindGenresByName(string name, int count)
        {
            IEnumerable<Genre> g = await genreDao.FindGenresByNameAsync(name);
            Assert.Equal(count, g.Count());
            if(count > 0)
            {
                Assert.True(g.All(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)));
            }
        }

        [Fact]
        public async Task UpdateTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Genre g = await genreDao.FindByIdAsync(8);
                Assert.NotNull(g);
                Assert.Equal("Thriller", g.Name);
                g.Name = "Update Name";
                Genre g1 = await genreDao.UpdateAsync(g);

                Assert.NotNull(g1);
                Assert.Equal("Update Name", g1.Name);

            } // do not commit transacionScope
        }

        [Fact]
        public async Task DeleteTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Genre g = await genreDao.FindByIdAsync(15);
                Assert.NotNull(g);
                Assert.Equal("Krimi", g.Name);
                await Assert.ThrowsAsync<AdoException>(async () => await genreDao.DeleteByIdAsync(15));

                Genre g1 = await genreDao.GetOrAddGenreByNameAsync("New Name");
                Assert.NotNull(g1);
                Assert.Equal("New Name", g1.Name);

                int count = await genreDao.DeleteByIdAsync(g1.Id);
                Assert.Equal(1, count);


                Genre g2 = await genreDao.FindByIdAsync(g1.Id);
                Assert.Null(g2);

            } // do not commit transacionScope
        }

        [Theory]
        [InlineData(2, 3, 6, "Genre1", "Genre2", "Genre3")]
        [InlineData(2, 3, 5, "Genre1", "Genre2", "Thriller")]
        [InlineData(2, 3, 3, "Fantasy", "Horror", "Thriller")]
        public async Task AddGenreToMovieAsyncTest(int movieId, int startCount, int finalCount, params string[] genres)
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Movie m1 = await movieDao.FindByIdAsync(movieId);
                Assert.NotNull(m1);
                Assert.Equal(startCount, m1.Genre.Count());

                IList<Genre> dbGenres = await GetGenresAsync(genres);
                await genreDao.AddGenreToMovieAsync(m1, dbGenres);

                Movie m2 = await movieDao.FindByIdAsync(movieId);
                Assert.Equal(finalCount, m2.Genre.Count());

                Assert.True(genres.All(t => m2.Genre.Select(t => t.Name).Contains(t)));
            } // do not commit transacionScope
        }

        private async Task<IList<Genre>> GetGenresAsync(string[] genres)
        {
            IList<Genre> retVal = new List<Genre>();
            foreach (string g in genres)
            {
                retVal.Add(await genreDao.GetOrAddGenreByNameAsync(g));
            }
            return retVal;
        }

        [Theory]
        [InlineData(7, 4, 4, "Musik")]
        [InlineData(7, 4, 3, "Familie", "Krimi")]
        [InlineData(7, 4, 0, "Animation", "Familie", "Komödie", "Abenteuer")]
        public async Task RemoveGenreFromMovieAsyncTest(int movieId, int startCount, int finalCount, params string[] genres)
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Movie m1 = await movieDao.FindByIdAsync(movieId);
                Assert.NotNull(m1);
                Assert.Equal(startCount, m1.Genre.Count());

                IList<Genre> dbGenres = await GetGenresAsync(genres);
                await genreDao.RemoveGenreFromMovieAsync(m1, dbGenres);

                Movie m2 = await movieDao.FindByIdAsync(movieId);
                Assert.Equal(finalCount, m2.Genre.Count());

                Assert.True(genres.All(t => !m2.Genre.Select(t => t.Name).Contains(t)));
            } // do not commit transacionScope
        }
    }
}
