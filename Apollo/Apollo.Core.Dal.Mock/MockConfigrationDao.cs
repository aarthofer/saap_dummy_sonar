using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    // https://exceptionnotfound.net/using-moq-to-create-fluent-test-classes-in-asp-net-core/
    public class MockConfigrationDao : Mock<IConfigurationDao>
    {
        private List<Configuration> configs = new List<Configuration>()
        {
            new Configuration(){ Key = "key1", JsonValue= "{value: 'test1'}"},
            new Configuration(){ Key = "key2", JsonValue= "{value: 'test2'}"},
            new Configuration(){ Key = "key3", JsonValue= "{value: 'test3'}"},
            new Configuration(){ Key = "Constraint", JsonValue = "{Active: 'true', Strategy: 'Apollo.Plugins.OneReservationPerRow'}"}
        };

        public void Init()
        {
            MockUpdateAsync();
            MockCreateAsync();
            MockDeleteByPKAsync();
            MockFindAllAsync();
            MockFindByKeyAsync();
        }

        public MockConfigrationDao MockUpdateAsync()
        {
            Setup(r => r.UpdateAsync(It.IsAny<Configuration>()))
                .Returns((Configuration input) =>
                {
                    var r = configs.FirstOrDefault(x => x.Key == input.Key);
                    r.JsonValue = input.JsonValue;
                    return Task.FromResult(r);
                });
            return this;
        }

        public MockConfigrationDao MockCreateAsync()
        {
            Setup(r => r.CreateAsync(It.IsAny<Configuration>()))
                .Returns((Configuration input) =>
                {
                    configs.Add(input);
                    return Task.FromResult(input);
                });
            return this;
        }

        public MockConfigrationDao MockFindByKeyAsync()
        {
            Setup(r => r.FindByKeyAsync(It.IsAny<string>()))
                .Returns((string input) =>
                {
                    return Task.FromResult(configs.FirstOrDefault(x => x.Key == input));
                });
            return this;
        }

        public MockConfigrationDao MockDeleteByPKAsync()
        {
            Setup(r => r.DeleteByPKAsync(It.IsAny<Configuration>()))
                .Returns((Configuration input) =>
                {
                    var r = configs.FirstOrDefault(x => x.Key == input.Key);
                    if (r != null)
                    {
                        configs.Remove(r);
                        return Task.FromResult(1);
                    }
                    else
                    {
                        return Task.FromResult(0);
                    }
                });
            return this;
        }

        public MockConfigrationDao MockFindAllAsync()
        {
            Setup(r => r.FindAllAsync())
                .Returns(() =>
                {
                    return Task.FromResult(configs.AsEnumerable());
                });
            return this;
        }
    }
}
