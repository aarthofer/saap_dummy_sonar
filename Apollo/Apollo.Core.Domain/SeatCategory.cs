using System;

namespace Apollo.Core.Domain
{
    [TableName("category")]
    public class SeatCategory
    {
        [Column("id", true), PK]
        public int Id { get; set; }

        [Column("cinemaId")]
        public int CinemaId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("price")]
        public Double Price { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            SeatCategory other = (SeatCategory)obj;

            if (!Name.Equals(other.Name) || Math.Abs(Price - other.Price) > Price * 0.00001)
            {
                return false;
            }

            return true;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Name.GetHashCode() + Price.GetHashCode();
        }

        public override string ToString()
        {
            return $"{nameof(SeatCategory)}-> id: {Id}, name:  {Name}, price: {Price}";
        }
    }
}
