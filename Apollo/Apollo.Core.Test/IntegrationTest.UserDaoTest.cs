using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace Apollo.Core.Test
{
    public class UserDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IUserDao userDao;

        public UserDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));

            userDao = new UserDao(connectionFactory, qbFactory);
        }

        [Theory]
        [InlineData("Testuser", "secure password", "teat@test.at", User.UserRole.USER)]
        [InlineData("Testmanageäöü?r", "not secure#ä+ß password", "manage?är@test.at", User.UserRole.MANAGER)]
        public async Task InsertUserTest(string name, string password, string email, User.UserRole role)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                User u = new User()
                {
                    Username = name,
                    Password = password,
                    Email = email,
                    Role = role
                };

                User u1 = await userDao.CreateAsync(u);
                Assert.NotNull(u1);
                Assert.Equal(name, u1.Username);
                Assert.Equal(password, u1.Password);
                Assert.Equal(email, u1.Email);
                Assert.Equal(role, u1.Role);
            }
        }

        [Theory]
        [InlineData("Testuser", "secure password", "teat@test.at", User.UserRole.USER)]
        [InlineData("Testmanageäöü?r", "not secure#ä+ß password", "manage?är@test.at", User.UserRole.MANAGER)]
        public async Task UpdateUserTest(string name, string password, string email, User.UserRole role)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                User u = new User()
                {
                    Username = name,
                    Password = password,
                    Email = email,
                    Role = role
                };

                User u1 = await userDao.CreateAsync(u);
                Assert.NotNull(u1);
                Assert.Equal(name, u1.Username);
                Assert.Equal(password, u1.Password);
                Assert.Equal(email, u1.Email);
                Assert.Equal(role, u1.Role);

                u1.Username = "Username";
                u1.Email = "Email";
                u1.Password = "Password";
                u1.Role = role.Equals(User.UserRole.MANAGER) ? User.UserRole.USER : User.UserRole.MANAGER;

                User uu = await userDao.UpdateAsync(u1);
                Assert.NotNull(uu);

                User u2 = await userDao.FindByIdAsync(uu.Id);

                Assert.Equal(u1.Username, u2.Username);
                Assert.Equal(u1.Password, u2.Password);
                Assert.Equal(u1.Email, u2.Email);
                Assert.Equal(u1.Role, u2.Role);
                Assert.NotEqual(u.Role, u2.Role);
            }
        }

        [Theory]
        [InlineData("Testuser", "secure password", "teat@test.at", User.UserRole.USER)]
        public async Task DeleteUser(string name, string password, string email, User.UserRole role)
        {
            using (TransactionScope scope = TestUtils.CreateTransaction())
            {
                User u = new User()
                {
                    Username = name,
                    Password = password,
                    Email = email,
                    Role = role
                };

                u = await userDao.CreateAsync(u);

                User u1 = await userDao.FindByIdAsync(u.Id);

                Assert.NotNull(u1);
                Assert.Equal(name, u1.Username);
                Assert.Equal(password, u1.Password);
                Assert.Equal(email, u1.Email);
                Assert.Equal(role, u1.Role);

                int i = await userDao.DeleteByIdAsync(u.Id);
                Assert.Equal(1, i);

                User notFound = await userDao.FindByIdAsync(u.Id);
                Assert.Null(notFound);
            }
        }
    }
}
