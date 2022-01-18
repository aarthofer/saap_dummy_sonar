using Apollo.BLInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.BL.Test
{
    public class ConstraintTest
    {
        [Fact]
        public async Task TestNoConstraintDefaultPlugin()
        {
            IEnumerable<ReservationSeat> seats = GetSeats();
            IConstraintBl coronaBl = new ConstraintBl();

            var reservationSeats = await coronaBl.CalculateSeatConstraints(seats);

            foreach (ReservationSeat seat in reservationSeats)
            {
                Assert.Equal(ReservationSeat.State.free, seat.state);
            }
        }

        [Fact]
        public async Task TestPlugin()
        {
            IEnumerable<ReservationSeat> seats = GetSeats();
            seats.Where(seat => seat.Seat.Row == 1 && seat.Seat.Col == 1).Single().state = ReservationSeat.State.reserved;

            IConstraintBl coronaBl = new ConstraintBl();

            var strategies = coronaBl.GetConstraintStrategies();

            coronaBl.SetConstraintStrategy(strategies.First());

            var reservationSeats = await coronaBl.CalculateSeatConstraints(seats);

            foreach (ReservationSeat seat in reservationSeats)
            {
                if (seat.Seat.Row == 1)
                {
                    Assert.NotEqual(ReservationSeat.State.free, seat.state);
                }
                else
                {
                    Assert.Equal(ReservationSeat.State.free, seat.state);
                }
                
            }
        }

        [Fact]
        public void TestSetConstraintStrategy()
        {
            IConstraintBl coronaBl = new ConstraintBl();
            Type strategy = typeof(ConstraintStrategies.NoConstraintsStrategy).Assembly.GetType("Apollo.BL.ConstraintStrategies.NoConstraintsStrategy");
            
            coronaBl.SetConstraintStrategy(strategy);
        }

        [Fact]
        public void TestSetConstraintStrategyNull()
        {
            IConstraintBl coronaBl = new ConstraintBl();
            Type strategy = null;

            Assert.Throws<ArgumentNullException>(() => coronaBl.SetConstraintStrategy(strategy));
        }

        [Fact]
        public void TestSetConstraintStrategyInterfaceNotImplemented()
        {
            IConstraintBl coronaBl = new ConstraintBl();
            Type strategy = typeof(ConstraintStrategies.NoConstraintsStrategy).Assembly.GetType("Apollo.BL.ConstraintBl");

            Assert.Throws<ArgumentException>(() => coronaBl.SetConstraintStrategy(strategy));
        }

        [Fact]
        public async Task TestSetConstraintStrategyString()
        {
            IConstraintBl coronaBl = new ConstraintBl();
            coronaBl.SetConstraintStrategy("Apollo.Plugins.OneReservationPerRow");

            IEnumerable<ReservationSeat> seats = GetSeats();
            seats.Where(seat => seat.Seat.Row == 1 && seat.Seat.Col == 1).Single().state = ReservationSeat.State.reserved;

            foreach (ReservationSeat seat in await coronaBl.CalculateSeatConstraints(seats))
            {
                if (seat.Seat.Row == 1)
                {
                    Assert.NotEqual(ReservationSeat.State.free, seat.state);
                }
                else
                {
                    Assert.Equal(ReservationSeat.State.free, seat.state);
                }
            }
        }

        [Fact]
        public void TestSetConstraintStrategyStringNotExists()
        {
            IConstraintBl coronaBl = new ConstraintBl();
            Assert.Throws<InvalidOperationException>(() => coronaBl.SetConstraintStrategy("x"));
        }

        private IEnumerable<ReservationSeat> GetSeats()
        {
            List<ReservationSeat> seats = new List<ReservationSeat>();
            seats.Add(new ReservationSeat { Seat = new Core.Domain.CinemaHallSeat { Id = 1, SeatNr = 1, Row = 1, Col = 1, cinemaHallId = 1, CategoryId = 1 }, state = ReservationSeat.State.free});
            seats.Add(new ReservationSeat { Seat = new Core.Domain.CinemaHallSeat { Id = 1, SeatNr = 2, Row = 1, Col = 2, cinemaHallId = 1, CategoryId = 1 }, state = ReservationSeat.State.free });
            seats.Add(new ReservationSeat { Seat = new Core.Domain.CinemaHallSeat { Id = 1, SeatNr = 3, Row = 1, Col = 3, cinemaHallId = 1, CategoryId = 1 }, state = ReservationSeat.State.free });

            seats.Add(new ReservationSeat { Seat = new Core.Domain.CinemaHallSeat { Id = 1, SeatNr = 4, Row = 2, Col = 1, cinemaHallId = 1, CategoryId = 1 }, state = ReservationSeat.State.free });
            seats.Add(new ReservationSeat { Seat = new Core.Domain.CinemaHallSeat { Id = 1, SeatNr = 5, Row = 2, Col = 2, cinemaHallId = 1, CategoryId = 1 }, state = ReservationSeat.State.free });
            seats.Add(new ReservationSeat { Seat = new Core.Domain.CinemaHallSeat { Id = 1, SeatNr = 6, Row = 2, Col = 3, cinemaHallId = 1, CategoryId = 1 }, state = ReservationSeat.State.free });
            return seats;
        }
    }
}
