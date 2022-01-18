using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IScheduleDao : IApolloDao<Schedule>
    {
        Task<IEnumerable<Schedule>> GetScheduleForDay(DateTime date, int cinemaId);

        Task<IEnumerable<Schedule>> GetScheduleForDay(DateTime date, int cinemaId, int cinemaHallId);

        Task<IEnumerable<Schedule>> InsertSchedules(IEnumerable<Schedule> schedules);

        Task<IEnumerable<Schedule>> GetScheduleByMovieIdAsync(int movieId, DateTime from, DateTime to);

        Task<int> RemoveSchedulesForCinemaHall(int cinemaHallId);
    }
}
