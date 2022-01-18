using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IConstraintBl
    {
        public IEnumerable<Type> GetConstraintStrategies();
        public void SetConstraintStrategy(Type strategy);
        public void SetConstraintStrategy(String strategyType);
        public Task<IEnumerable<ReservationSeat>> CalculateSeatConstraints(IEnumerable<ReservationSeat> seats);
    }
}
