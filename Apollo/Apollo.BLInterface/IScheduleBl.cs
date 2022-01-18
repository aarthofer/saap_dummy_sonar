using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IScheduleBl
    { 
        Task<IEnumerable<Schedule>> GetSchedulesOfDay(DateTime day, int cinemaId);
        Task<IEnumerable<Schedule>> GetSchedulesOfDay(DateTime day, int cinemaId, int cinemaHallId);
        Task<IEnumerable<Schedule>> GetSchedulesOfDay(DateTime day, int cinemaId, IEnumerable<Genre> genres);
        Task<Schedule> CreateScheduleAsync(int cinemaId, Schedule schedule);
        Task<Schedule> GetScheduleByIdAsync(int scheduleId);
        Task<Schedule> UpdateSchedule(Schedule schedule);
        Task<int> DeleteSchedule(int scheduleId);
        Task<int> DeleteSchedule(Schedule schedule);

        Task<IEnumerable<Schedule>> GetScheduleByMovieIdAsync(int movieId, DateTime from, DateTime to);
    }
}
