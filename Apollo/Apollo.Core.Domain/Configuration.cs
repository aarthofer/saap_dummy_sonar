using Newtonsoft.Json;

namespace Apollo.Core.Domain
{
    [TableName("configuration")]
    public class Configuration
    {
        [Column("key"), PK]
        public string Key { get; set; }

        [Column("value")]
        public string JsonValue
        {
            get
            {
                return JsonConvert.SerializeObject(Value);
            }
            set
            {
                object val = JsonConvert.DeserializeObject(value);
                if (val != null)
                {
                    Value = val;
                }
            }
        }

        public object Value { get; set; }
    }
}
