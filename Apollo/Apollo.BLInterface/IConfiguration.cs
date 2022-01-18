using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IConfiguration
    {
        Task<string> GetValueJsonString(string key);
        Task AddOrUpdateValue<T>(string key, T jsonValue) where T : ISerializable;

        Task RemoveKey(string key);

        object DeserializeValue(string jsonValue);
    }
}
