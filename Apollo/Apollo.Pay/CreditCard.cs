using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Pay
{
    public class CreditCard
    {
        public string Number { get; set; }
        public string Owner { get; set; }
        public ExpirationDate ExpirationDate { get; set; }
        public string ValidationCode { get; set; }
    }
}
