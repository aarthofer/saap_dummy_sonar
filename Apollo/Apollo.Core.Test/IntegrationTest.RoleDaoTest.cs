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
    public class RoleDaoTest
    {
        private IConnectionFactory connectionFactory;
        private IQueryBuilderFactory qbFactory;
        private IRoleDao roleDao;

        public RoleDaoTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var dbConfig = config.GetSection("ConnectionStrings")
                .GetSection("ApolloDbConnection");

            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var providerName = dbConfig.GetValue<string>("ProviderName");

            connectionFactory = new DefaultConnectionFactory(connectionString, providerName);
            qbFactory = new DefaultQueryBuilderFactory(dbConfig.GetValue<string>("QueryBuilder"));
            roleDao = new RoleDao(connectionFactory, qbFactory);
        }

        [Theory]
        [InlineData("3D Director", 527)]
        [InlineData("TestRole", -1)]
        [InlineData("ADR & Dubbing", 246)]
        [InlineData("adr & dubbing", 246)]
        public async Task CreateRoleTest(string name, int id)
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Role g = await roleDao.GetOrAddRoleByNameAsync(name);
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

        [Fact]
        public async Task UpdateTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Role g = await roleDao.FindByIdAsync(8);
                Assert.NotNull(g);
                Assert.Equal("Director", g.Name);
                g.Name = "Update Name";
                Role g1 = await roleDao.UpdateAsync(g);

                Assert.NotNull(g1);
                Assert.Equal("Update Name", g1.Name);

            } // do not commit transacionScope
        }

        [Fact]
        public async Task DeleteTest()
        {
            using (TransactionScope transactionScope = TestUtils.CreateTransaction())
            {
                Role g = await roleDao.FindByIdAsync(15);
                Assert.NotNull(g);
                Assert.Equal("Co-Producer", g.Name);
                await Assert.ThrowsAsync<AdoException>(async () => await roleDao.DeleteByIdAsync(15));

                Role g1 = await roleDao.GetOrAddRoleByNameAsync("New Name");
                Assert.NotNull(g1);
                Assert.Equal("New Name", g1.Name);

                int count = await roleDao.DeleteByIdAsync(g1.Id);
                Assert.Equal(1, count);


                Role g2 = await roleDao.FindByIdAsync(g1.Id);
                Assert.Null(g2);

            } // do not commit transacionScope
        }
    }
}
