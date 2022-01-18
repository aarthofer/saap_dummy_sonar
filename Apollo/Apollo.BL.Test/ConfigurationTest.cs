using Apollo.BLInterface;
using Apollo.Core.Dal.Mock;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.BL.Test
{
    public class ConfigurationTest
    {
        IConfiguration configurationBl;
        MockConfigrationDao configrationDao = new MockConfigrationDao();

        public ConfigurationTest()
        {
            configrationDao.Init();

            configurationBl = new ConfigurationBl(configrationDao.Object);
        }

        [Fact]
        public async Task TestGetKey()
        {
            string val = await configurationBl.GetValueJsonString("key1");
            Assert.Equal("{\"value\":\"test1\"}", val);

            object objVal = configurationBl.DeserializeValue(val);
            Assert.NotNull(objVal);
            Assert.Equal("test1", ((JObject)objVal).GetValue("value").ToString());
        }

        [Fact]
        public async Task TestGetKeyInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => configurationBl.GetValueJsonString(""));
            await Assert.ThrowsAsync<ArgumentException>(() => configurationBl.GetValueJsonString(null));
        }

        [Fact]
        public async Task TestDeserializeInvalid()
        {
            Assert.Throws<ArgumentException>(() => configurationBl.DeserializeValue(""));
            Assert.Throws<ArgumentException>(() => configurationBl.DeserializeValue(null));
        }

        [Fact]
        public async Task TestDeleteKey()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => configurationBl.RemoveKey(""));
            await Assert.ThrowsAsync<ArgumentException>(() => configurationBl.RemoveKey(null));

            string val1 = await configurationBl.GetValueJsonString("key1");
            Assert.Equal("{\"value\":\"test1\"}", val1);

            string val2 = await configurationBl.GetValueJsonString("key2");
            Assert.NotNull(val2);

            await configurationBl.RemoveKey("key1");

            string nval1 = await configurationBl.GetValueJsonString("key1");
            Assert.Null(nval1);

            string nval2 = await configurationBl.GetValueJsonString("key2");
            Assert.NotNull(nval2);
        }

        [Fact]
        public async Task TestAddKey()
        {
            string val1 = await configurationBl.GetValueJsonString("key99");
            Assert.Null(val1);

            await configurationBl.AddOrUpdateValue("key99", new TestSerializable { Name = "test" });

            string nval1 = await configurationBl.GetValueJsonString("key99");
            Assert.NotNull(nval1);
            Assert.Equal("{\"Name\":\"test\"}", nval1);
        }

        [Fact]
        public async Task TestUpdateKey()
        {
            string val1 = await configurationBl.GetValueJsonString("key1");
            Assert.NotNull(val1);
            Assert.Equal("{\"value\":\"test1\"}", val1);

            await configurationBl.AddOrUpdateValue("key1", new TestSerializable { Name = "test" });

            string nval1 = await configurationBl.GetValueJsonString("key1");
            Assert.NotNull(nval1);
            Assert.Equal("{\"Name\":\"test\"}", nval1);
        }

        [Fact]
        public async Task TestUpdateKeyInvalid()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => configurationBl.AddOrUpdateValue("", new TestSerializable { Name = "test" }));
            await Assert.ThrowsAsync<ArgumentException>(() => configurationBl.AddOrUpdateValue(null, new TestSerializable { Name = "test" }));


            await Assert.ThrowsAsync<ArgumentNullException>(() => configurationBl.AddOrUpdateValue<TestSerializable>("key1", null));

        }

        public class TestSerializable : ISerializable
        {
            public string Name { get; set; }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                    throw new ArgumentNullException("info");

                info.AddValue("Name", Name);
            }
        }
    }
}
