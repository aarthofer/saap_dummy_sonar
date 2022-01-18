using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    public class MockReservationDao : Mock<IReservationDao>
    {
        public MockReservationDao MockFindById(Reservation reservation)
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(reservation));

            return this;
        }

        public MockReservationDao MockFindByInvalidID()
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<Reservation>(null));
            return this;
        }

        public MockReservationDao MockGetReservations(IEnumerable<Reservation> reservations)
        {
            Setup(x => x.GetReservations(It.IsAny<int>()))
                .Returns(Task.FromResult(reservations));

            return this;
        }

        public MockReservationDao MockUpdateReservation(Reservation reservation)
        {
            Setup(x => x.UpdateAsync(It.IsAny<Reservation>()))
                .Returns(Task.FromResult(reservation));
            return this;
        }

        public MockReservationDao MockRemoveReservation()
        {
            Setup(x => x.DeleteByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(1));
            return this;
        }

        public void VerifyGetReservations(Func<Times> times)
        {
            Verify(x => x.GetReservations(It.IsAny<int>()), times);
        }

        public void VerifyFindById(Func<Times> times)
        {
            Verify(x => x.FindByIdAsync(It.IsAny<int>()), times);
        }

        public void VerifyDeleteById(Func<Times> times)
        {
            Verify(x => x.DeleteByIdAsync(It.IsAny<int>()), times);
        }

        public void VerifyUpdate(Func<Times> times)
        {
            Verify(x => x.UpdateAsync(It.IsAny<Reservation>()), times);
        }
    }
}
