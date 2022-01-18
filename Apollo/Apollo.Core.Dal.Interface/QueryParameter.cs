namespace Apollo.Core.Dal.Interface
{
    public class QueryParameter
    {
        public QueryParameter(string paramName, object value)
        {
            ParamName = paramName;
            Value = value;
        }

        public string ParamName { get; }
        public object Value { get; set; }
    }
}
