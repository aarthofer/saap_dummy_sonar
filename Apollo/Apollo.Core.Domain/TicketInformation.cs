using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Core.Domain
{
    public class TicketInformation
    {
        public string MovieTitle { get; set; }
        public string Cinema { get; set; }
        public string Row { get; set; }
        public string Seat { get; set; }
        public DateTime StartTime { get; set; }
        public int ReservatoinId { get; set; }

        public string GetQRCodeString()
        {
            return $"{MovieTitle}|{Cinema}|{Row}|{Seat}|{ReservatoinId}";
        }
    }
}
