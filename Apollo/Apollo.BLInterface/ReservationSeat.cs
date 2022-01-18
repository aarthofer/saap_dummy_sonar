using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.BLInterface
{
    public class ReservationSeat
    {
        public enum State { free, reserved, selected, corridor };
        public CinemaHallSeat Seat { get; set; }
        public State state { get; set; } = State.free;

        public override string ToString()
        {
            return $"{nameof(ReservationSeat)} -> Seat: {Seat}, state: {state}";
        }
    }
}
