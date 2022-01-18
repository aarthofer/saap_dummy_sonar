using System;

namespace Apollo.Core.Dal.Interface
{
    public class AdoException : Exception
    {
        public AdoException(string message) : base(message)
        {
        }

        public AdoException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
