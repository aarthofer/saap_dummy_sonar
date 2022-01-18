using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class ConfigurationBl : IConfiguration
    {
        private readonly IConfigurationDao configurationDao;

        public ConfigurationBl(IConfigurationDao configurationDao)
        {
            this.configurationDao = configurationDao ?? throw new ArgumentNullException(nameof(configurationDao));
        }

        public async Task AddOrUpdateValue<T>(string key, T jsonValue) where T : ISerializable
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }

            if (jsonValue is null)
            {
                throw new ArgumentNullException(nameof(jsonValue));
            }

            Configuration c = await configurationDao.FindByKeyAsync(key);
            if (c == null)
            {
                await configurationDao.CreateAsync(new Core.Domain.Configuration()
                {
                    Key = key,
                    Value = jsonValue
                });
            }
            else
            {
                c.Value = jsonValue;
                await configurationDao.UpdateAsync(c);
            }
        }

        public object DeserializeValue(string jsonValue)
        {
            if (string.IsNullOrEmpty(jsonValue))
            {
                throw new ArgumentException($"'{nameof(jsonValue)}' cannot be null or empty", nameof(jsonValue));
            }

            return JsonConvert.DeserializeObject(jsonValue);
        }

        public async Task<string> GetValueJsonString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }

            return (await configurationDao.FindByKeyAsync(key))?.JsonValue;
        }

        public async Task RemoveKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty", nameof(key));
            }

            await configurationDao.DeleteByPKAsync(new Core.Domain.Configuration() { Key = key });
        }
    }
}
