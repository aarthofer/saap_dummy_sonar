using System;
using System.Collections.Generic;

namespace Apollo.Core.Domain
{
    [TableName("cinemaHall")]
    public class CinemaHall
    {
        [Column("id", true), PK]
        public int Id { get; set; } = -1;

        [Column("cinemaId")]
        public int CinemaId { get; set; } = -1;

        [Column("name")]
        public string Name { get; set; }

        public IEnumerable<CinemaHallSeat> Seats { get; set; }

        public override string ToString()
        {
            return $"{nameof(CinemaHall)}: id: {Id}, name:{Name}";
        }
    }
}