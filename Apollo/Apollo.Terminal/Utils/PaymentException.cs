using Apollo.Pay;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Terminal.Utils
{
    public class PaymentException : Exception
    {
        public PaymentResult Result { get; set; }

        public PaymentException(PaymentResult result) : base()
        {
            Result = result;
        }

        public PaymentException(PaymentResult result, string message) : base(message)
        {
            Result = result;
        }
    }
}
