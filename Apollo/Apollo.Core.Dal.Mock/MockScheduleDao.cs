using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    public class MockScheduleDao : Mock<IScheduleDao>
    {
        public MockScheduleDao MockCreate(Schedule schedule)
        {
            Setup(x => x.CreateAsync(It.IsAny<Schedule>()))
                .Returns(Task.FromResult(schedule));
            return this;
        }

        public MockScheduleDao MockDelete()
    {
            Setup(x => x.DeleteByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(1));
            return this;
        }
        public MockScheduleDao MockFindById(Schedule schedule)
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(schedule));
            return this;
        }

        public MockScheduleDao MockUpdate(Schedule schedule)
        {
            Setup(x => x.UpdateAsync(It.IsAny<Schedule>()))
                .Returns(Task.FromResult(schedule));
            return this;
        }

        public MockScheduleDao MockGetScheduleForCinemaForDay(IEnumerable<Schedule> schedule)
        {
            Setup(x => x.GetScheduleForDay(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(Task.FromResult(schedule));
            return this;
        }

        public MockScheduleDao MockGetScheduleForCinemaHallForDay(IEnumerable<Schedule> schedule)
        {
            Setup(x => x.GetScheduleForDay(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(schedule));
            return this;
        }

        public void VerifyGetScheduleForCinemaForDay(Func<Times> times)
        {
            Verify(x => x.GetScheduleForDay(It.IsAny<DateTime>(), It.IsAny<int>()), times);
        }

        public void VerifyGetScheduleForCinemaHallForDay(Func<Times> times)
        {
            Verify(x => x.GetScheduleForDay(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()), times);
        }

        public void VerifyFindById(Func<Times> times)
        {
            Verify(x => x.FindByIdAsync(It.IsAny<int>()), times);
        }

        public void VerifyCreateSchedule(Func<Times> times)
        {
            Verify(x => x.CreateAsync(It.IsAny<Schedule>()), times);
        }

        public void VerifyDeleteSchedule(Func<Times> times)
        {
            Verify(x => x.DeleteByIdAsync(It.IsAny<int>()), times);
        }

        public void VerifyUpdateSchedule(Func<Times> times)
        {
            Verify(x => x.UpdateAsync(It.IsAny<Schedule>()), times);
        }
    }
}
