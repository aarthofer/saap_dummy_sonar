using System.Collections.Generic;

namespace Apollo.Core.Domain
{
    [TableName("reservation")]
    public class Reservation
    {
        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("scheduleId")]
        public int ScheduleId { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("isPayed")]
        public bool IsPayed { get; set; }

        public IEnumerable<CinemaHallSeat> Seats { get; set; }

        public override string ToString()
        {
            return $"{nameof(Reservation)} -> id: {Id}, scheduleId: {ScheduleId}, userId: {UserId}, isPayed: {IsPayed}";
        }
    }
}
