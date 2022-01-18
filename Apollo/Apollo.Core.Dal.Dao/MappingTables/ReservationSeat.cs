using Apollo.Core.Domain;

namespace Apollo.Core.Dal.Dao.MappingTables
{
    [TableName("reservationSeat")]
    class ReservationSeat
    {

        [Column("reservationId"), PK]
        public int ReservationId { get; set; }

        [Column("hallSeatId"), PK]
        public int HallSeatId { get; set; }

    }
}
