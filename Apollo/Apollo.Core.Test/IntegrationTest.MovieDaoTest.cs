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
    public class MovieDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IPersonDao personDao;
        private IGenreDao genreDao;
        private IMovieDao movieDao;
        private IRoleDao roleDao;

        public MovieDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            genreDao = new GenreDao(connectionFactory, qbFactory);
            roleDao = new RoleDao(connectionFactory, qbFactory);
            personDao = new PersonDao(connectionFactory, qbFactory, roleDao);
            movieDao = new MovieDao(connectionFactory, qbFactory, genreDao, personDao);
        }

        [Theory]
        [InlineData(5, 3, 66, 19)]
        [InlineData(29, 3, 83, 223)]
        public async Task FindMovieBasicTest(int movieId, int genreCount, int actorCount, int crewCount)
        {
            Movie m = await movieDao.FindByIdAsync(movieId);
            Assert.NotNull(m);
            Assert.Equal(genreCount, m.Genre.Count());
            Assert.Equal(actorCount, m.Actors.Count());
            Assert.Equal(crewCount, m.Crew.Count());
        }

        [Fact]
        public async Task FindNoMovieBasicTest()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await movieDao.FindByIdAsync(-1));
        }

        [Fact]
        public async Task FindAllTest()
        {
            IEnumerable<Movie> movies = await movieDao.FindAllAsync();
            Assert.NotNull(movies);
            Assert.NotEmpty(movies);
            Assert.Equal(133, movies.Count());
            Assert.Equal("Colombiana", movies.Where(t => t.Id == 45).FirstOrDefault().Title);
            Assert.Equal(4, movies.Where(t => t.Id == 45).FirstOrDefault().Genre.Count());
            Assert.Equal(22, movies.Where(t => t.Id == 45).FirstOrDefault().Actors.Count());
            Assert.Equal(91, movies.Where(t => t.Id == 45).FirstOrDefault().Crew.Count());
        }

        [Theory]
        [InlineData("The new movie", "The movie description", 123, "2020-01-11", "path to image.jpg", "www.trailer.test")]
        public async Task InsertMovieOnlyTest(string title, string desc, int duration, DateTime releaseDate, string image, string trailer)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie m = new Movie()
                {
                    Title = title,
                    Description = desc,
                    DurationMinutes = duration,
                    ReleaseDate = releaseDate,
                    Trailer = trailer,
                    Image = image
                };

                m = await movieDao.CreateAsync(m);
                Assert.NotNull(m);
                Assert.Equal(title, m.Title);
                Assert.Equal(desc, m.Description);
                Assert.Equal(duration, m.DurationMinutes);
                Assert.Equal(releaseDate, m.ReleaseDate);
                Assert.Equal(trailer, m.Trailer);
                Assert.Equal(image, m.Image);
                Assert.Empty(m.Crew);
                Assert.Empty(m.Actors);
                Assert.Empty(m.Genre);
            } // TransactionScope do not commit!
        }

        [Theory]
        [InlineData("The new movie", "The movie description", 123, "2020-01-11", "www.trailer.test")]
        public async Task InsertMovieFullTest(string title, string desc, int duration, DateTime releaseDate, string trailer)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie m = new Movie()
                {
                    Title = title,
                    Description = desc,
                    DurationMinutes = duration,
                    ReleaseDate = releaseDate,
                    Trailer = trailer
                };

                Person timberlake = (await personDao.FindPersonsByNameAsync("Timberlake")).FirstOrDefault();
                Person corden = (await personDao.FindPersonsByNameAsync("James Corden")).FirstOrDefault();

                Assert.NotNull(timberlake);
                Assert.NotNull(corden);

                m.Actors = new List<Person>{
                    new Person() { Name = "Hans Krankl" },
                    new Person() { Name = "Sepp Mustermann" },
                    new Person() { Name = "James Corden" },
                    timberlake
                };

                Genre komodie = await genreDao.FindByIdAsync(3);
                Genre drama = await genreDao.FindByIdAsync(10);

                Assert.NotNull(komodie);
                Assert.NotNull(drama);

                m.Genre = new List<Genre>{
                    new Genre() { Name = "SWK" },
                    new Genre() { Name = "Komödie" },
                    drama
                };

                Role editor = await roleDao.FindByIdAsync(5);
                Role Screenplay = await roleDao.FindByIdAsync(4);

                Assert.NotNull(editor);
                Assert.NotNull(Screenplay);

                m.Crew = new List<KeyValuePair<Person, string>>{
                    new KeyValuePair<Person, string>(new Person() { Name = "Hans Krankl" }, "Screenplay"),
                    new KeyValuePair<Person, string>(new Person() { Name = "Sepp Mustermann" }, "Co-Director"),
                    new KeyValuePair<Person, string>(new Person() { Name = "James Corden" }, editor.Name),
                    new KeyValuePair<Person, string>(timberlake, "Gute Frage"),
                };

                m = await movieDao.CreateAsync(m);
                Assert.NotNull(m);
                Assert.Equal(title, m.Title);
                Assert.Equal(desc, m.Description);
                Assert.Equal(duration, m.DurationMinutes);
                Assert.Equal(releaseDate, m.ReleaseDate);
                Assert.Equal(trailer, m.Trailer);
                Assert.NotEmpty(m.Actors);
                Assert.NotEmpty(m.Crew);
                Assert.NotEmpty(m.Genre);
            } // TransactionScope do not commit!
        }

        [Theory]
        [InlineData(1, "Trolls World Tour", 94, "2020-03-12", "/6kJkuYpTvKP8AXw2EAG5WJFDxpf.jpg")]
        public async Task UpdateMovieFullTest(int id, string title, int duration, DateTime releaseDate, string trailer)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie movie = await movieDao.FindByIdAsync(id);
                Assert.NotNull(movie);
                Assert.Equal(title, movie.Title);
                Assert.Equal(duration, movie.DurationMinutes);
                Assert.Equal(releaseDate, movie.ReleaseDate);

                Person timberlake = (await personDao.FindPersonsByNameAsync("Timberlake")).FirstOrDefault();
                Person corden = (await personDao.FindPersonsByNameAsync("James Corden")).FirstOrDefault();
                Person homsy = (await personDao.FindPersonsByNameAsync("Stephen Homsy")).FirstOrDefault();
                Person berger = (await personDao.FindPersonsByNameAsync("Glenn Berger")).FirstOrDefault();

                Assert.NotNull(timberlake);
                Assert.NotNull(corden);
                Assert.NotNull(homsy);
                Assert.NotNull(berger);

                Assert.True(movie.Actors.Contains(timberlake));
                Assert.True(movie.Actors.Contains(corden));
                Assert.False(movie.Actors.Contains(homsy));

                Genre animation = await genreDao.FindByIdAsync(1);
                Genre familie = await genreDao.FindByIdAsync(2);
                Genre drama = await genreDao.FindByIdAsync(10);

                Assert.NotNull(animation);
                Assert.NotNull(familie);
                Assert.NotNull(drama);

                Assert.True(movie.Genre.Contains(animation));
                Assert.True(movie.Genre.Contains(familie));
                Assert.False(movie.Genre.Contains(drama));

                // ************ Movie Updates *******************

                movie.Title = "update titel";
                movie.Description = " update desc";
                movie.DurationMinutes = 99;
                movie.ReleaseDate = new DateTime(2000, 8, 8);
                movie.Trailer = "update trailer";

                var tmpGenre = movie.Genre.ToList();
                tmpGenre.Remove(familie);
                tmpGenre.Add(drama);
                movie.Genre = tmpGenre;

                var tmpActors = movie.Actors.ToList();
                tmpActors.Remove(timberlake);
                tmpActors.Add(homsy);
                movie.Actors = tmpActors;

                var tmpCrew = movie.Crew.ToList();
                tmpCrew.Remove(new KeyValuePair<Person, string>(timberlake, "Executive Music Producer"));
                tmpCrew.Add(new KeyValuePair<Person, string>(homsy, "Screenplay"));

                tmpCrew.Remove(new KeyValuePair<Person, string>(berger, "Screenplay"));
                tmpCrew.Add(new KeyValuePair<Person, string>(berger, "Story"));

                movie.Crew = tmpCrew;

                // **********************************************

                Movie m1 = await movieDao.UpdateAsync(movie);
                Assert.NotNull(m1);
                Assert.Equal(movie.Title, m1.Title);
                Assert.Equal(movie.Description, m1.Description);
                Assert.Equal(movie.DurationMinutes, m1.DurationMinutes);
                Assert.Equal(movie.ReleaseDate, m1.ReleaseDate);
                Assert.Equal(movie.Trailer, m1.Trailer);

                Assert.NotEmpty(m1.Actors);
                Assert.NotEmpty(m1.Crew);
                Assert.NotEmpty(m1.Genre);

                Assert.True(m1.Genre.Contains(animation));
                Assert.False(m1.Genre.Contains(familie));
                Assert.True(m1.Genre.Contains(drama));

                Assert.True(m1.Actors.Contains(corden));
                Assert.False(m1.Actors.Contains(timberlake));
                Assert.True(m1.Actors.Contains(homsy));

                Assert.False(m1.Crew.Contains(new KeyValuePair<Person, string>(timberlake, "Executive Music Producer")));
                Assert.True(m1.Crew.Contains(new KeyValuePair<Person, string>(homsy, "Screenplay")));

                Assert.False(m1.Crew.Contains(new KeyValuePair<Person, string>(berger, "Screenplay")));
                Assert.True(m1.Crew.Contains(new KeyValuePair<Person, string>(berger, "Story")));
            } // TransactionScope do not commit!
        }

        [Theory]
        [InlineData(133)]
        public async Task DeleteMovieTest(int movieId)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie m = await movieDao.FindByIdAsync(movieId);
                Assert.NotNull(m);
                int deleteCount = await movieDao.DeleteByIdAsync(m.Id);
                Assert.Equal(1, deleteCount);
                await Assert.ThrowsAsync<ArgumentException>(async () => await movieDao.FindByIdAsync(movieId));
            }// TransactionScope do not commit!

            Movie m1 = await movieDao.FindByIdAsync(movieId);
            Assert.NotNull(m1);
        }

        [Theory]
        [InlineData("James Bond", 3)]
        [InlineData("NoMovie", 0)]
        public async Task TestSearchMovieByTitle(string title, int count)
        {
            var result = await movieDao.SearchMovies(title, "", false, null, null, null);
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }

        [Theory]
        [InlineData("Daniel Craig", 3, true)]
        [InlineData("Timberlake", 1, true)]
        [InlineData("timberlake", 1, false)]
        [InlineData("imberla", 1, true)]
        [InlineData("Will Smith", 4, false)]
        [InlineData("will smith", 3, true)]
        [InlineData("noPerson", 0, true)]
        [InlineData("noPerson", 0, false)]
        public async Task TestSearchMovieByPersonName(string name, int count, bool onlyActors)
        {
            var result = await movieDao.SearchMovies("", name, onlyActors, null, null, null);
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }

        [Theory]
        [InlineData(4266, 3, true)]
        [InlineData(2, 1, true)]
        [InlineData(2, 1, false)]
        [InlineData(6701, 4, false)]
        [InlineData(6701, 3, true)]
        [InlineData(999999, 0, true)]
        [InlineData(-1, 0, true)]
        public async Task TestSearchMovieByPersonId(int? id, int count, bool onlyActors)
        {
            var result = await movieDao.SearchMovies("", id, onlyActors, null, null, null);
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }

        [Theory]
        [InlineData(2, 22)]
        [InlineData(10, 50)]
        [InlineData(999999, 0)]
        [InlineData(-1, 0)]
        public async Task TestSearchMovieByGenreId(int? id, int count)
        {
            var result = await movieDao.SearchMovies("", null, false, id, null, null);
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }

        [Theory]
        [InlineData("Liebesfilm", 15)]
        [InlineData("Thriller", 45)]
        [InlineData("noGenre", 0)]
        public async Task TestSearchMovieByGenreName(string name, int count)
        {
            var result = await movieDao.SearchMovies("", null, false, name, null, null);
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }

        [Theory]
        [InlineData(null, null, 133)]
        [InlineData("2020-01-01", null, 4)]
        [InlineData(null, "1990-12-31", 2)]
        [InlineData("2010-01-01", "2010-12-31", 5)]
        [InlineData("2010-12-31", "2010-01-01", 0)]
        public async Task TestSearchMovieByDate(string from, string to, int count)
        {
            var result = await movieDao.SearchMovies("", "", false, "",
                string.IsNullOrEmpty(from) ? null : (DateTime?)DateTime.Parse(from),
                string.IsNullOrEmpty(to) ? null : (DateTime?)DateTime.Parse(to));
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }

        [Theory]
        [InlineData("Bond", "Craig", true, "Action", "2000-01-01", "2010-12-31", 2)]
        [InlineData("InvalidTitle", "Craig", true, "Action", "2000-01-01", "2010-12-31", 0)]
        [InlineData("Bond", "InvalidActor", true, "Action", "2000-01-01", "2010-12-31", 0)]
        [InlineData("Bond", "Craig", true, "InvalidAction", "2000-01-01", "2010-12-31", 0)]
        public async Task TestSearchMovie(string title, string personName, bool onlyActors, string genreName, string from, string to, int count)
        {
            var result = await movieDao.SearchMovies(title, personName, onlyActors, genreName,
                string.IsNullOrEmpty(from) ? null : (DateTime?)DateTime.Parse(from),
                string.IsNullOrEmpty(to) ? null : (DateTime?)DateTime.Parse(to));
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }

        [Theory]
        [InlineData("Bond", 4266, true, 11, "2000-01-01", "2010-12-31", 2)]
        [InlineData("InvalidTitle", 4266, true, 11, "2000-01-01", "2010-12-31", 0)]
        [InlineData("Bond", 99999, true, 11, "2000-01-01", "2010-12-31", 0)]
        [InlineData("Bond", 4266, true, 9999, "2000-01-01", "2010-12-31", 0)]
        public async Task TestSearchMovieByIds(string title, int personName, bool onlyActors, int genreName, string from, string to, int count)
        {
            var result = await movieDao.SearchMovies(title, personName, onlyActors, genreName,
                string.IsNullOrEmpty(from) ? null : (DateTime?)DateTime.Parse(from),
                string.IsNullOrEmpty(to) ? null : (DateTime?)DateTime.Parse(to));
            Assert.Equal(count, result.Count());
            if (count > 0)
            {
                Assert.NotEmpty(result.First().Actors);
                Assert.NotEmpty(result.First().Crew);
            }
        }
    }
}
