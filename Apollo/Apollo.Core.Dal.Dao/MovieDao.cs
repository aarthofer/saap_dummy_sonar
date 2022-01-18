using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao.MappingTables;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Apollo.Core.Dal.Dao
{
    public class MovieDao : ApolloDao<Movie>, IMovieDao
    {
        private readonly IGenreDao genreDao;
        private readonly IPersonDao personDao;

        public MovieDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory, IGenreDao genreDao, IPersonDao personDao) : base(connectionFactory, qbFactory)
        {
            this.genreDao = genreDao ?? throw new ArgumentNullException(nameof(genreDao));
            this.personDao = personDao ?? throw new ArgumentNullException(nameof(personDao));
        }

        public override async Task<IEnumerable<T>> FindAllAsync<T>()
        {
            if (typeof(T) != typeof(Movie))
            {
                return await base.FindAllAsync<T>();
            }

            IEnumerable<Movie> movies = await base.FindAllAsync<Movie>();

            foreach (var movie in movies)
            {
                Task.WaitAll(FillGenreAsync(movie), FillActorsAsync(movie), FillCrewAsync(movie));
            }

            return movies.Cast<T>();
        }

        public override async Task<T> FindByIdAsync<T>(int id)
        {
            if (typeof(T) != typeof(Movie))
            {
                return await base.FindByIdAsync<T>(id);
            }

            Movie movie = await base.FindByIdAsync<Movie>(id);

            if (movie == null)
            {
                throw new ArgumentException($"No movie with ID {id} found");
            }
            Task.WaitAll(FillGenreAsync(movie), FillActorsAsync(movie), FillCrewAsync(movie));

            return (T)Convert.ChangeType(movie, typeof(T));
        }

        public async override Task<T> CreateAsync<T>(T obj)
        {
            if (typeof(T) != typeof(Movie))
            {
                return await base.CreateAsync<T>(obj);
            }
            using (TransactionScope transactionScope = new TransactionScope())
            {
                Movie movie = (Movie)Convert.ChangeType(obj, typeof(Movie));

                IEnumerable<Genre> genres = await GetCreateGenresAsync(movie.Genre);
                IEnumerable<Person> actors = await GetCreateActorsAsync(movie.Actors);
                IEnumerable<KeyValuePair<Person, string>> crew = await GetCreateCrewAsync(movie.Crew);

                Movie dbMovie = await base.CreateAsync<Movie>(movie);
                await genreDao.AddGenreToMovieAsync(dbMovie, genres);
                await personDao.AddActorsToMovieAsync(dbMovie, actors);
                await personDao.AddCrewToMovieAsync(dbMovie, crew);

                Movie fullMovie = await FindByIdAsync<Movie>(dbMovie.Id);
                transactionScope.Complete();

                return (T)Convert.ChangeType(fullMovie, typeof(T));
            }
        }

        public async override Task<T> UpdateAsync<T>(T obj)
        {
            if (typeof(T) != typeof(Movie))
            {
                return await base.UpdateAsync<T>(obj);
            }
            Movie movie = (Movie)Convert.ChangeType(obj, typeof(Movie));

            Movie dbMovie = await FindByIdAsync(movie.Id);
            if (dbMovie == null)
            {
                throw new ArgumentException("move does not exist in db: " + movie);
            }

            await UpdateGenreAsync(movie, dbMovie);
            await UpdateActorsAsync(movie, dbMovie);
            await UpdateCrewAsync(movie, dbMovie);


            Movie retMovie = await base.UpdateAsync<Movie>(movie);
            return (T)Convert.ChangeType(retMovie, typeof(T));
        }

        public async override Task<T> FindByPrimaryKey<T>(T obj)
        {
            if (typeof(T) != typeof(Movie))
            {
                return await base.FindByPrimaryKey(obj);
            }
            Movie movie = (Movie)Convert.ChangeType(obj, typeof(Movie));
            Movie retMovie = await FindByIdAsync<Movie>(movie.Id);
            return (T)Convert.ChangeType(retMovie, typeof(T));
        }

        private async Task<IList<KeyValuePair<Person, string>>> GetCreateCrewAsync(IEnumerable<KeyValuePair<Person, string>> crew)
        {
            IList<KeyValuePair<Person, string>> crewList = new List<KeyValuePair<Person, string>>();
            foreach (var c in crew)
            {
                Person c1 = await personDao.GetOrAddPersonByNameAsync(c.Key.Name);
                crewList.Add(new KeyValuePair<Person, string>(c1, c.Value));
            }
            return crewList;
        }

        private async Task<IList<Person>> GetCreateActorsAsync(IEnumerable<Person> actors)
        {
            IList<Person> actorList = new List<Person>();
            foreach (var a in actors)
            {
                Person a1 = await personDao.GetOrAddPersonByNameAsync(a.Name);
                actorList.Add(a1);
            }
            return actorList;
        }

        private async Task<IList<Genre>> GetCreateGenresAsync(IEnumerable<Genre> genre)
        {
            IList<Genre> genres = new List<Genre>();
            foreach (var g in genre)
            {
                Genre g1 = await genreDao.GetOrAddGenreByNameAsync(g.Name);
                genres.Add(g1);
            }
            return genres;
        }

        public async override Task<int> DeleteByIdAsync(int id)
        {
            var deleteGenreQuery = GetQueryBuilder()
                .QueryType(QueryTypeEnum.DELETE)
                .Table(typeof(MovieGenre))
                .AddAnd(Column.Create<MovieGenre>("movieId"), OperationType.Equals, id);

            var deletePersonMovieQuery = GetQueryBuilder()
                .QueryType(QueryTypeEnum.DELETE)
                .Table(typeof(PersonMovie))
                .AddAnd(Column.Create<PersonMovie>("movieId"), OperationType.Equals, id);

            Task.WaitAll(template.ExecuteAsync(deleteGenreQuery), template.ExecuteAsync(deletePersonMovieQuery));

            return await base.DeleteByIdAsync(id);
        }

        private async Task FillGenreAsync(Movie movie)
        {
            movie.Genre = await genreDao.FindGenresByMovieIdAsync(movie.Id);
        }

        private async Task FillActorsAsync(Movie movie)
        {
            movie.Actors = await personDao.FindActorsByMovieIdAsync(movie.Id);
        }

        private async Task FillCrewAsync(Movie movie)
        {
            movie.Crew = await personDao.FindCrewByMovieIdAsync(movie.Id);
        }

        private async Task UpdateCrewAsync(Movie update, Movie existing)
        {
            List<KeyValuePair<Person, string>> toDelete = existing.Crew.Except(update.Crew).ToList();
            List<KeyValuePair<Person, string>> newCrew = update.Crew.Except(existing.Crew).ToList();
            await personDao.RemoveCrewFromMovieAsync(existing, toDelete);
            await personDao.AddCrewToMovieAsync(existing, newCrew);
        }

        private async Task UpdateActorsAsync(Movie update, Movie existing)
        {
            List<Person> toDelete = existing.Actors.Except(update.Actors).ToList();
            List<Person> newActors = update.Actors.Except(existing.Actors).ToList();
            await personDao.RemoveActorsFromMovieAsync(existing, toDelete);
            await personDao.AddActorsToMovieAsync(existing, newActors);
        }

        private async Task UpdateGenreAsync(Movie update, Movie existing)
        {
            List<Genre> toDelete = existing.Genre.Except(update.Genre).ToList();
            List<Genre> newGenres = update.Genre.Except(existing.Genre).ToList();
            await genreDao.RemoveGenreFromMovieAsync(existing, toDelete);
            await genreDao.AddGenreToMovieAsync(existing, newGenres);
        }

        public async Task<IEnumerable<Movie>> SearchMovies(string title, string personName, bool onlyActors, string genreName, DateTime? from, DateTime? to)
        {
            var movieSearch = GetQueryBuilder()
                   .Table(typeof(Movie));

            SearchMoviesByTitle(movieSearch, title);
            SearchMoviesByPerson(movieSearch, personName, onlyActors);
            SearchMoviesByGenre(movieSearch, genreName);
            ReleaseDateSearch(movieSearch, from, to);

            return await DistinctAndLoadAdditionals(await template.QueryAsync(movieSearch, template.GenericRowMapper<Movie>));
        }

        public async Task<IEnumerable<Movie>> SearchMovies(string title, int? personId, bool onlyActors, int? genreId, DateTime? from, DateTime? to)
        {
            var movieSearch = GetQueryBuilder()
                   .Table(typeof(Movie));

            SearchMoviesByTitle(movieSearch, title);
            SearchMoviesByPerson(movieSearch, personId, onlyActors);
            SearchMoviesByGenre(movieSearch, genreId);
            ReleaseDateSearch(movieSearch, from, to);

            return await DistinctAndLoadAdditionals(await template.QueryAsync(movieSearch, template.GenericRowMapper<Movie>));
        }

        private void SearchMoviesByTitle(IQueryBuilder movieSearch, string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                movieSearch.AddAnd(Column.Create<Movie>("title"), OperationType.Like, title);
            }
        }

        private void SearchMoviesByPerson(IQueryBuilder movieSearch, string name, bool onlyActors)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            movieSearch
                .JoinTable(Column.Create<Movie>("id"), Column.Create<PersonMovie>("movieid"))
                .JoinTable(Column.Create<PersonMovie>("personid"), Column.Create<Person>("id"))
                .AddAnd(Column.Create<Person>("name", "personname"), OperationType.Like, name, "personname");

            if (onlyActors)
            {
                movieSearch.JoinTable(Column.Create<PersonMovie>("roleid"), Column.Create<Role>("id"));
                movieSearch.AddAnd(Column.Create<Role>("name", "rolename"), OperationType.Equals, PersonDao.ACTOR_ROLE_NAME, "rolename");
            }
        }

        private void SearchMoviesByPerson(IQueryBuilder movieSearch, int? personId, bool onlyActors)
        {
            if (personId == null)
            {
                return;
            }

            movieSearch
                .JoinTable(Column.Create<Movie>("id"), Column.Create<PersonMovie>("movieid"))
                .JoinTable(Column.Create<PersonMovie>("personid"), Column.Create<Person>("id"))
                .AddAnd(Column.Create<PersonMovie>("personid"), OperationType.Equals, personId);

            if (onlyActors)
            {
                movieSearch.JoinTable(Column.Create<PersonMovie>("roleid"), Column.Create<Role>("id"));
                movieSearch.AddAnd(Column.Create<Role>("name", "rolename"), OperationType.Equals, PersonDao.ACTOR_ROLE_NAME, "rolename");
            }
        }

        private void SearchMoviesByGenre(IQueryBuilder movieSearch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            movieSearch
                   .JoinTable(Column.Create<Movie>("id"), Column.Create<MovieGenre>("movieid"))
                   .JoinTable(Column.Create<MovieGenre>("genreid"), Column.Create<Genre>("id"))
                   .AddAnd(Column.Create<Genre>("name", "genrename"), OperationType.Like, name, "genrename");
        }

        private void SearchMoviesByGenre(IQueryBuilder movieSearch, int? genreId)
        {
            if (genreId == null)
            {
                return;
            }

            movieSearch
                .JoinTable(Column.Create<Movie>("id"), Column.Create<MovieGenre>("movieid"))
                .AddAnd(Column.Create<MovieGenre>("genreid", "genreid"), OperationType.Equals, genreId, "genreid");
        }

        private void ReleaseDateSearch(IQueryBuilder movieSearch, DateTime? from, DateTime? to)
        {
            if (from != null)
            {
                movieSearch.AddAnd(Column.Create<Movie>("releasedate", "dateFrom"), OperationType.GreaterEqualThen, from, "dateFrom");
            }

            if (to != null)
            {
                movieSearch.AddAnd(Column.Create<Movie>("releasedate", "dateTo"), OperationType.LessThanEqual, to, "dateTo");
            }

        }

        private async Task<IEnumerable<Movie>> DistinctAndLoadAdditionals(IEnumerable<Movie> movies)
        {
            List<Movie> fullMovies = new List<Movie>();
            foreach (var movie in (movies).Distinct(new MovieIdComparer()))
            {
                fullMovies.Add(await FindByIdAsync(movie.Id));
            }
            return fullMovies;
        }
    }
}