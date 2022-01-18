using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Core.Domain
{
    [TableName("cinema")]
    public class Cinema
    {
        [Column("id", true), PK]
        public int Id { get; set; } = -1;

        [Column("name")]
        public string Name { get; set; }

        public IEnumerable<SeatCategory> Categories { get; set; }

        public IEnumerable<CinemaHall> CinemaHalls { get; set; } = new List<CinemaHall>();
    }
}
