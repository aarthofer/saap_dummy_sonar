using Apollo.BLInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class ConstraintBl : IConstraintBl
    {
        private Type Strategy;

        public async Task<IEnumerable<ReservationSeat>> CalculateSeatConstraints(IEnumerable<ReservationSeat> seats)
        {
            if (Strategy == null)
            {
                IConstraint strategy = new ConstraintStrategies.NoConstraintsStrategy();
                return await strategy.CalculateConstraints(seats);
            }

            ConstructorInfo ctor = Strategy.GetConstructor(new Type[0]);
            IConstraint constraint = (IConstraint)ctor.Invoke(null);

            return await constraint.CalculateConstraints(seats);
        }

        public IEnumerable<Type> GetConstraintStrategies()
        {
            return GetConstraintStrategiesWhere(t => 1 == 1);
        }

        public IEnumerable<Type> GetConstraintStrategiesWhere(Predicate<Type> predicate)
        {
            String[] pluginFiles = System.IO.Directory.GetFiles($"ConstraintStrategies");

            List<Type> plugins = new List<Type>();

            foreach (String file in pluginFiles)
            {
                Assembly reflectionPlugin = Assembly.LoadFrom(file);

                foreach (Type type in reflectionPlugin.GetTypes())
                {
                    if (type.GetInterface("IConstraint") == null || !predicate.Invoke(type))
                    {
                        continue;
                    }

                    plugins.Add(type);               
                }
            }

            return plugins;
        }

        public void SetConstraintStrategy(Type strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException("strategy cannot be null");
            }

            if(strategy.GetInterface("IConstraint") == null)
            {
                throw new ArgumentException("Current strategy does not implement IConstraint interface");
            }

            Strategy = strategy;
        }

        public void SetConstraintStrategy(String strategyType)
        {
            Strategy = GetConstraintStrategiesWhere(t => String.Equals(t.ToString(),strategyType)).Single(); 
        }
    }
}
