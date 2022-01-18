using System;

namespace Apollo.Core.Domain
{
    [TableName("schedule")]
    public class Schedule
    {
        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("cinemaHallId")]
        public int CinemaHallId { get; set; }

        [Column("movieId")]
        public int MovieId { get; set; }

        public Movie Movie { get; set; }

        public CinemaHall CinemaHall { get; set; }

        [Column("startTime")]
        public DateTime StartTime { get; set; }

        public override string ToString()
        {
            return $"{Id}; cinemaHallId: {CinemaHallId}, movieId{MovieId}, startTime{StartTime}";
        }
    }
}