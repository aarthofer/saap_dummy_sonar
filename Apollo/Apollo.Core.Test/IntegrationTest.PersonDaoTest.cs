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
    public class PersonDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IPersonDao personDao;
        private IMovieDao movieDao;
        private IRoleDao roleDao;

        public PersonDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            roleDao = new RoleDao(connectionFactory, qbFactory);
            personDao = new PersonDao(connectionFactory, qbFactory, roleDao);
            movieDao = new MovieDao(connectionFactory, qbFactory, new GenreDao(connectionFactory, qbFactory), personDao);
        }

        [Fact]
        public async Task TestFindPersonBasic()
        {
            Assert.NotEmpty(await personDao.FindAllAsync());

            Assert.NotNull(await personDao.FindByIdAsync(2));
        }



        [Fact]
        public async Task SimplePersonDaoTest()
        {
            Person person = await personDao.FindByIdAsync(2);

            Assert.True(2 == person.Id);
            Assert.Equal("Justin Timberlake", person.Name);
        }

        [Fact]
        public async Task SimplePersonDaoInsertTest()
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Person person = new Person();
                person.Name = "Hans Krankl";

                var task1 = personDao.FindAllAsync();

                await task1;
                await personDao.CreateAsync(person);

                var task2Result = await personDao.FindAllAsync();

                Assert.NotNull(task1.Result);
                Assert.NotNull(task2Result);

                Assert.True(task1.Result.Count<Person>() < task2Result.Count<Person>());
            }
        }

        [Theory]
        [InlineData("xxxxxxxxxxx", 0)]
        [InlineData("Justin Timberlake", 1)]
        [InlineData("justin timberlake", 1)]
        [InlineData("mike", 75)]
        public async Task SimplePersonFindByNameTest(string name, int count)
        {
            var result = await personDao.FindPersonsByNameAsync(name);
            Assert.Equal(count, result.Count());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SimplePersonFindByEmptyNameTest(string name)
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await personDao.FindPersonsByNameAsync(name));
        }

        [Theory]
        [InlineData("New Unique Person")]
        public async Task TestTransactionRollback(string name)
        {
            var result = await personDao.FindPersonsByNameAsync(name);
            Assert.Empty(result);

            Person p = new Person()
            {
                Name = name
            };
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                await personDao.CreateAsync(p);

                result = await personDao.FindPersonsByNameAsync(name);
                Assert.NotEmpty(result);

                // do not complete Transaction
                //scope.Complete();
            }
            result = await personDao.FindPersonsByNameAsync(name);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("New Commited Person")]
        public async Task TestTransactionCommit(string name)
        {
            int id = -1;
            try
            {
                var result = await personDao.FindPersonsByNameAsync(name);
                Assert.Empty(result);

                Person p = new Person()
                {
                    Name = name
                };

                using (TransactionScope scope = TestUtils.CreateTransaction())
                {
                    p = await personDao.CreateAsync(p);
                    Assert.NotEqual(0, p.Id);

                    result = await personDao.FindPersonsByNameAsync(name);
                    Assert.NotEmpty(result);
                    id = result.First().Id;
                    Assert.Equal(p.Id, id);

                    scope.Complete();
                }

                result = await personDao.FindPersonsByNameAsync(name);
                Assert.NotEmpty(result);
            }
            finally
            {
                if (id > 0)
                {
                    int count = await personDao.DeleteByIdAsync(id);
                    Assert.Equal(1, count);
                }
            }
        }

        [Theory]
        [InlineData(5, "Helen Aberson", 67)]
        [InlineData(5, "Bret Jones", 66)]
        public async Task AddActorToMovieAsyncTest(int movieId, string name, int actorsCount)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie m = await movieDao.FindByIdAsync(movieId);
                Assert.NotNull(m);
                Person p = (await personDao.FindPersonsByNameAsync(name)).FirstOrDefault();
                Assert.NotNull(p);
                await personDao.AddActorsToMovieAsync(m, new List<Person> { p });
                Movie m1 = await movieDao.FindByIdAsync(movieId);
                Assert.Equal(actorsCount, m1.Actors.Count());
            }
        }

        [Theory]
        [InlineData(29, "Erick Hayden", 82)]
        [InlineData(29, "Pattye Rogers", 83)]
        public async Task RemoveActorFromMovieAsyncTest(int movieId, string name, int actorsCount)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie m = await movieDao.FindByIdAsync(movieId);
                Assert.NotNull(m);
                Person p = (await personDao.FindPersonsByNameAsync(name)).FirstOrDefault();
                Assert.NotNull(p);
                await personDao.RemoveActorsFromMovieAsync(m, new List<Person> { p });
                Movie m1 = await movieDao.FindByIdAsync(movieId);
                Assert.Equal(actorsCount, m1.Actors.Count());
            }
        }

        [Theory]
        [InlineData(5, "Rick Heinrichs", 16, 19)]
        [InlineData(5, "Bret Jones", 86, 20)]
        public async Task AddCrewToMovieTest(int movieId, string name, int roleId, int crewCount)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie m = await movieDao.FindByIdAsync(movieId);
                Assert.NotNull(m);

                Person p = (await personDao.FindPersonsByNameAsync(name)).FirstOrDefault();
                Assert.NotNull(p);

                Role r = await roleDao.FindByIdAsync(roleId);
                Assert.NotNull(r);

                await personDao.AddCrewToMovieAsync(m, new List<KeyValuePair<Person, string>> { new KeyValuePair<Person, string>(p, r.Name) });
                Movie m1 = await movieDao.FindByIdAsync(movieId);
                Assert.Equal(crewCount, m1.Crew.Count());
                Assert.Single(m1.Crew.Where(t => t.Key.Equals(p) && t.Value == r.Name));
            }
        }

        [Theory]
        [InlineData(37, "Trevor Gates", 37, 60)]
        [InlineData(37, "Trevor Gates", 2, 61)]
        [InlineData(37, "Angie Luckey", 86, 61)]
        [InlineData(37, "Angie Luckey", 61, 61)]
        public async Task RemoveCrewFromMovieAsyncTest(int movieId, string name, int roleId, int crewCount)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Movie m = await movieDao.FindByIdAsync(movieId);
                Assert.NotNull(m);

                Person p = (await personDao.FindPersonsByNameAsync(name)).FirstOrDefault();
                Assert.NotNull(p);

                Role r = await roleDao.FindByIdAsync(roleId);
                Assert.NotNull(r);

                await personDao.RemoveCrewFromMovieAsync(m, new List<KeyValuePair<Person, string>> { new KeyValuePair<Person, string>(p, r.Name) });
                Movie m1 = await movieDao.FindByIdAsync(movieId);
                Assert.Equal(crewCount, m1.Crew.Count());
                Assert.Empty(m1.Crew.Where(t => t.Key.Equals(p) && t.Value == r.Name));
            }
        }

        [Theory]
        [InlineData("Ehren Kruger", 310)]
        [InlineData("Dummy Person", -1)]
        public async Task GetOrAddPersonByNameTestAsync(string name, int id)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                Person p = await personDao.GetOrAddPersonByNameAsync(name);
                Assert.NotNull(p);
                if (id < 0)
                {
                    Assert.True(p.Id > 1);
                }
                else
                {
                    Assert.Equal(id, p.Id);
                }
            }
        }
    }
}
