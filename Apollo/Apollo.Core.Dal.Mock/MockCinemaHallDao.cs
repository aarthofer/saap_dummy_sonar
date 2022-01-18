using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    public class MockCinemaHallDao : Mock<ICinemaHallDao>
    {
       public MockCinemaHallDao MockFindById(CinemaHall hall)
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(hall));
            return this;
        }

        public MockCinemaHallDao MockGetCinemaHallSeats(IEnumerable<CinemaHallSeat> seats)
        {
            Setup(x => x.GetCinemaHallSeats(It.IsAny<int>()))
                .Returns(Task.FromResult(seats));
            return this;
        }

        public void VerifyFindById(Func<Times> times)
        {
            Verify(x => x.FindByIdAsync(It.IsAny<int>()), times);
        }

        public void VerifyGetSeats(Func<Times> times)
        {
            Verify(x => x.GetCinemaHallSeats(It.IsAny<int>()), times);
        }
    }
}
