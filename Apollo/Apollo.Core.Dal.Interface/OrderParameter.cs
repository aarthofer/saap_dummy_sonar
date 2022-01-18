using System;

namespace Apollo.Core.Dal.Interface
{
    public class OrderParameter : Column
    {
        public OrderParameter(Type domainObject, string column, string direction) : base(domainObject, column)
        {
            Direction = direction;
        }
        public string Direction { get; set; }

        public static new OrderParameter Create<T>(string name, string direction = "ASC")
        {
            return new OrderParameter(typeof(T), name, direction);
        }
    }
}
