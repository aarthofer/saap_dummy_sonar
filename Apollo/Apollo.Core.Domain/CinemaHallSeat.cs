using System;

namespace Apollo.Core.Domain
{
    [TableName("hallSeat")]
    public class CinemaHallSeat
    {

        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("seatNr")]
        public int SeatNr { get; set; }

        [Column("cinemaHallId")]
        public int cinemaHallId { get; set; }

        [Column("categoryId")]
        public int CategoryId { get; set; }

        [Column("row")]
        public int Row { get; set; }

        [Column("col")]
        public int Col { get; set; }

        public override bool Equals(object obj)
        {
            return obj is CinemaHallSeat seat &&
                   SeatNr == seat.SeatNr &&
                   cinemaHallId == seat.cinemaHallId &&
                   Row == seat.Row &&
                   Col == seat.Col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SeatNr, cinemaHallId, Row, Col);
        }

        public override string ToString()
        {
            return $"{nameof(CinemaHallSeat)} -> id: {Id}, cinemaHallId: {cinemaHallId}, seatNr: {SeatNr}, categoryId: {CategoryId}, row: {Row}, col: {Col}";
        }

        public CinemaHallSeat Clone()
        {
            return new CinemaHallSeat
            {
                Id = this.Id,
                cinemaHallId = this.cinemaHallId,
                CategoryId = this.CategoryId,
                Col = this.Col,
                Row = this.Row,
                SeatNr = this.SeatNr
            };
        }
    }
}