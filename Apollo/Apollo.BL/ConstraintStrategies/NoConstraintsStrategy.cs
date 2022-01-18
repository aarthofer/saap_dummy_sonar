using Apollo.BLInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL.ConstraintStrategies
{
    public class NoConstraintsStrategy : IConstraint
    {
        public Task<IEnumerable<ReservationSeat>> CalculateConstraints(IEnumerable<ReservationSeat> seats)
        {
            return Task.FromResult(seats);
        }
    }
}
