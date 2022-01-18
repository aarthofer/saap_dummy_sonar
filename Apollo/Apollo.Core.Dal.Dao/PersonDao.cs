using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao.MappingTables;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Dao
{
    public class PersonDao : ApolloDao<Person>, IPersonDao
    {
        public const string ACTOR_ROLE_NAME = "Actor";
        private readonly IRoleDao roleDao;

        public PersonDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory, IRoleDao roleDao) : base(connectionFactory, qbFactory)
        {
            this.roleDao = roleDao ?? throw new ArgumentNullException(nameof(roleDao));
        }

        private IQueryBuilder createJoinedQB()
        {
            return GetQueryBuilder()
                .Table(typeof(Person))
                .SelectColumn(Column.Create<Person>("*"))
                .SelectColumn(Column.Create<Role>("name", "role"))
                .JoinTable(Column.Create<Person>("id"), Column.Create<PersonMovie>("personId"))
                .JoinTable(Column.Create<PersonMovie>("movieId"), Column.Create<Movie>("id"))
                .JoinTable(Column.Create<PersonMovie>("roleId"), Column.Create<MovieRole>("id"));
        }

        public async Task<IEnumerable<Person>> FindActorsByMovieIdAsync(int id)
        {
            var joinedQB = createJoinedQB().SetCondition(Column.Create<MovieRole>("name"), OperationType.Equals, ACTOR_ROLE_NAME)
                .AddAnd(Column.Create<Movie>("id"), OperationType.Equals, id);

            return await template.QueryAsync(joinedQB, template.GenericRowMapper<Person>);
        }

        public async Task<IEnumerable<KeyValuePair<Person, string>>> FindCrewByMovieIdAsync(int id)
        {
            var joinedQB = createJoinedQB().SetCondition(Column.Create<MovieRole>("name"), OperationType.NotEquals, ACTOR_ROLE_NAME)
                .AddAnd(Column.Create<Movie>("id"), OperationType.Equals, id);

            return await template.QueryAsync(joinedQB, RowMapper);
        }

        private KeyValuePair<Person, string> RowMapper(IDataRecord row)
        {
            Person p = template.GenericRowMapper<Person>(row);
            string role = (string)row["role"];
            return new KeyValuePair<Person, string>(p, role);
        }

        public async Task<IEnumerable<Person>> FindPersonsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("No name specified");
            }

            var queryBuilder = GetQueryBuilder()
                .Table(typeof(Person))
                .SetCondition(new Column(typeof(Person), "name"), OperationType.Like, name);

            return await template.QueryAsync(queryBuilder, template.GenericRowMapper<Person>);
        }

        public async Task AddActorsToMovieAsync(Movie movie, IEnumerable<Person> actors)
        {
            if (movie == null || movie.Id < 0)
            {
                throw new ArgumentException("invalid movie id: " + movie);
            }

            Role actorRole = await roleDao.GetOrAddRoleByNameAsync(ACTOR_ROLE_NAME);

            foreach (var a in actors)
            {
                if (a == null)
                {
                    throw new ArgumentException("actor is null");
                }
                else if (a.Id < 0)
                {
                    Person newPerson = await GetOrAddPersonByNameAsync(a.Name);
                    a.Id = newPerson.Id;
                    a.Name = newPerson.Name;
                }

                if (movie.Actors.Contains(a))
                {
                    continue;
                }

                var moviePersonInserQuery = GetQueryBuilder()
                    .QueryType(QueryTypeEnum.INSERT)
                    .Table(typeof(PersonMovie))
                    .SetColumn(Column.Create<PersonMovie>("personId"), a.Id)
                    .SetColumn(Column.Create<PersonMovie>("movieId"), movie.Id)
                    .SetColumn(Column.Create<PersonMovie>("roleId"), actorRole.Id);
                await template.ExecuteAsync(moviePersonInserQuery);
            }
        }

        public async Task AddCrewToMovieAsync(Movie movie, IEnumerable<KeyValuePair<Person, string>> crew)
        {
            if (movie == null || movie.Id < 0)
            {
                throw new ArgumentException("invalid movie id: " + movie);
            }

            IDictionary<string, Role> roles = new Dictionary<string, Role>();

            foreach (var c in crew)
            {
                if (string.IsNullOrEmpty(c.Value))
                {
                    throw new ArgumentException("no crew role specified");
                }

                if (c.Key == null)
                {
                    throw new ArgumentException("crew-person is null");
                }
                else if (c.Key.Id < 0)
                {
                    Person newPerson = await GetOrAddPersonByNameAsync(c.Key.Name);
                    c.Key.Id = newPerson.Id;
                    c.Key.Name = newPerson.Name;
                }

                Role r = null;
                if (roles.ContainsKey(c.Value))
                {
                    r = roles[c.Value];
                }
                else
                {
                    r = await roleDao.GetOrAddRoleByNameAsync(c.Value);
                }

                if (movie.Crew.Where(t => t.Key.Equals(c.Key) && t.Value == r.Name).Any())
                {
                    continue;
                }

                var moviePersonInserQuery = GetQueryBuilder()
                    .QueryType(QueryTypeEnum.INSERT)
                    .Table(typeof(PersonMovie))
                    .SetColumn(Column.Create<PersonMovie>("personId"), c.Key.Id)
                    .SetColumn(Column.Create<PersonMovie>("movieId"), movie.Id)
                    .SetColumn(Column.Create<PersonMovie>("roleId"), r.Id);
                await template.ExecuteAsync(moviePersonInserQuery);
            }
        }

        public async Task<Person> GetOrAddPersonByNameAsync(string name)
        {
            var findQuery = GetQueryBuilder()
                .Table(typeof(Person))
                .AddAnd(Column.Create<Person>("name"), OperationType.Like, name);
            IEnumerable<Person> p = await template.QueryAsync<Person>(findQuery, template.GenericRowMapper<Person>);
            var single = p.Where(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (single != null)
            {
                return single;
            }
            else
            {
                return await CreateAsync(new Person() { Name = name });
            }
        }

        public async Task RemoveActorsFromMovieAsync(Movie movie, List<Person> actors)
        {
            if (movie == null || movie.Id < 0)
            {
                throw new ArgumentException("invalid movie id: " + movie);
            }

            Role actorRole = await roleDao.GetOrAddRoleByNameAsync(ACTOR_ROLE_NAME);

            foreach (var a in actors)
            {
                if (a == null || a.Id < 0)
                {
                    throw new ArgumentException("invalid actor id: " + a);
                }

                if (!movie.Actors.Contains(a))
                {
                    continue;
                }

                var moviePersonInserQuery = GetQueryBuilder()
                    .QueryType(QueryTypeEnum.DELETE)
                    .Table(typeof(PersonMovie))
                    .SetCondition(Column.Create<PersonMovie>("personId"), OperationType.Equals, a.Id)
                    .AddAnd(Column.Create<PersonMovie>("movieId"), OperationType.Equals, movie.Id)
                    .AddAnd(Column.Create<PersonMovie>("roleId"), OperationType.Equals, actorRole.Id);
                await template.ExecuteAsync(moviePersonInserQuery);
            }
        }

        public async Task RemoveCrewFromMovieAsync(Movie movie, List<KeyValuePair<Person, string>> crew)
        {
            if (movie == null || movie.Id < 0)
            {
                throw new ArgumentException("invalid movie id: " + movie);
            }

            IDictionary<string, Role> roles = new Dictionary<string, Role>();

            foreach (var c in crew)
            {
                if (c.Key == null || c.Key.Id < 0)
                {
                    throw new ArgumentException("invalid actor id: " + c);
                }

                if (string.IsNullOrEmpty(c.Value))
                {
                    throw new ArgumentException("no crew role specified");
                }

                Role r = null;
                if (roles.ContainsKey(c.Value))
                {
                    r = roles[c.Value];
                }
                else
                {
                    r = await roleDao.GetOrAddRoleByNameAsync(c.Value);
                }

                if (!movie.Crew.Where(t => t.Key.Equals(c.Key) && t.Value == r.Name).Any())
                {
                    continue;
                }

                var moviePersonInserQuery = GetQueryBuilder()
                    .QueryType(QueryTypeEnum.DELETE)
                    .Table(typeof(PersonMovie))
                    .SetCondition(Column.Create<PersonMovie>("personId"), OperationType.Equals, c.Key.Id)
                    .AddAnd(Column.Create<PersonMovie>("movieId"), OperationType.Equals, movie.Id)
                    .AddAnd(Column.Create<PersonMovie>("roleId"), OperationType.Equals, r.Id);
                await template.ExecuteAsync(moviePersonInserQuery);
            }
        }
    }
}