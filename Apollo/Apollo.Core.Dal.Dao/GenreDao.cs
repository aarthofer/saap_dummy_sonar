using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao.MappingTables;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Dao
{
    public class GenreDao : ApolloDao<Genre>, IGenreDao
    {
        public GenreDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory) : base(connectionFactory, qbFactory)
        {
        }

        public async Task AddGenreToMovieAsync(Movie movie, IEnumerable<Genre> genres)
        {
            if (movie == null || movie.Id < 0)
            {
                throw new ArgumentException("invalid movie id: " + movie);
            }

            foreach (var g in genres)
            {
                if (g == null)
                {
                    throw new ArgumentException("genre is null");
                }

                if(g.Id < 0)
                {
                    Genre tmp = await GetOrAddGenreByNameAsync(g.Name);
                    g.Id = tmp.Id;
                    g.Name = tmp.Name;
                }


                if (movie.Genre.Any(t => t.Name.Equals(g.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var movieGenreInserQuery = GetQueryBuilder()
                    .QueryType(QueryTypeEnum.INSERT)
                    .Table(typeof(MovieGenre))
                    .SetColumn(Column.Create<MovieGenre>("genreId"), g.Id)
                    .SetColumn(Column.Create<MovieGenre>("movieId"), movie.Id);
                await template.ExecuteAsync(movieGenreInserQuery);
            }
        }

        public async Task<IEnumerable<Genre>> FindGenresByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            IQueryBuilder qb = GetQueryBuilder()
                   .Table(typeof(Genre))
                   .SetCondition(Column.Create<Genre>("name"), OperationType.Like, name);

            return await template.QueryAsync(qb, template.GenericRowMapper<Genre>);
        }

        public async Task<IEnumerable<Genre>> FindGenresByMovieIdAsync(int id)
        {
            IQueryBuilder qb = GetQueryBuilder()
                .Table(typeof(Genre))
                .SelectColumn(Column.Create<Genre>("*"))
                .JoinTable(Column.Create<Genre>("id"), Column.Create<MovieGenre>("genreId"))
                .SetCondition(Column.Create<MovieGenre>("movieId"), OperationType.Equals, id);

            return await template.QueryAsync(qb, template.GenericRowMapper<Genre>);
        }

        public async Task<Genre> GetOrAddGenreByNameAsync(string name)
        {
            var findQuery = GetQueryBuilder()
                .Table(typeof(Genre))
                .AddAnd(Column.Create<Genre>("name"), OperationType.Like, name);
            IEnumerable<Genre> g = await template.QueryAsync<Genre>(findQuery, template.GenericRowMapper<Genre>);
            var single = g.Where(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (single != null)
            {
                return single;
            }
            else
            {
                return await this.CreateAsync(new Genre() { Name = name });
            }
        }

        public async Task RemoveGenreFromMovieAsync(Movie movie, IList<Genre> genres)
        {
            if (movie == null || movie.Id < 0)
            {
                throw new ArgumentException("invalid movie id: " + movie);
            }

            foreach (var g in genres)
            {
                if (g == null)
                {
                    throw new ArgumentException("invalid genre id: " + g);
                }

                if (movie.Genre.All(t => !t.Name.Equals(g.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var movieGenreInserQuery = GetQueryBuilder()
                    .QueryType(QueryTypeEnum.DELETE)
                    .Table(typeof(MovieGenre))
                    .SetCondition(Column.Create<MovieGenre>("genreId"), OperationType.Equals, g.Id)
                    .AddAnd(Column.Create<MovieGenre>("movieId"), OperationType.Equals, movie.Id);
                await template.ExecuteAsync(movieGenreInserQuery);
            }
        }
    }
}