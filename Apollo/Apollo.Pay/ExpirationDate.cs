using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Pay
{
    public class ExpirationDate
    {
        public ExpirationDate(int month, int year)
        {
            Month = month;
            Year = year;
        }

        public int Month { get; set; }
        public int Year { get; set; }

    }
}
