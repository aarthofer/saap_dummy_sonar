using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IConstraint
    {
        public Task<IEnumerable<ReservationSeat>> CalculateConstraints(IEnumerable<ReservationSeat> seats);
    }
}
