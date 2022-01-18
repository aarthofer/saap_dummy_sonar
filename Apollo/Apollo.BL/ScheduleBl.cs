using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class ScheduleBl : IScheduleBl
    {
        private readonly IScheduleDao ScheduleDao;
        private readonly ICinemaHallDao CinemaHallDao;
        private readonly IMovieDao MovieDao;

        public ScheduleBl(IScheduleDao scheduleDao, ICinemaHallDao cinemaHallDao, IMovieDao movieDao)
        {
            this.ScheduleDao = scheduleDao;
            this.CinemaHallDao = cinemaHallDao;
            this.MovieDao = movieDao;
        }
        public async Task<Schedule> CreateScheduleAsync(int cinemaId, Schedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException("Schedule cannot be null");
            }

            if (schedule.MovieId < 0 || schedule.CinemaHallId < 0 || cinemaId < 0)
            {
                throw new ArgumentException("MovieId, CinemaHallId and cinemaId have to be >= 0");
            }

            IEnumerable<Schedule> schedulesOfDay = await ScheduleDao.GetScheduleForDay(schedule.StartTime, cinemaId, schedule.CinemaHallId);

            await CheckValidStartTime(schedule, schedulesOfDay);

            return await ScheduleDao.CreateAsync(schedule);
        }

        public async Task<int> DeleteSchedule(int scheduleId)
        {
            if (scheduleId < 0)
            {
                throw new ArgumentException("Schedule Id has to be >= 0");
            }

            return await ScheduleDao.DeleteByIdAsync(scheduleId);
        }

        public async Task<int> DeleteSchedule(Schedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException("Schedule cannot be null");
            }

            return await DeleteSchedule(schedule.Id);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesOfDay(DateTime day, int cinemaId)
        {
            if (cinemaId < 0)
            {
                throw new ArgumentException("Cinema Id has to be >= 0");
            }

            var schedules = await ScheduleDao.GetScheduleForDay(day, cinemaId);

            if (schedules == null)
            {
                return new List<Schedule>();
            }

            return schedules;
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesOfDay(DateTime day, int cinemaId, int cinemaHallId)
        {
            if (cinemaId < 0)
            {
                throw new ArgumentException("Cinema Id has to be >= 0");
            }

            if (cinemaHallId < 0)
            {
                throw new ArgumentException("CinemaHall Id has to be >= 0");
            }

            var schedules = await ScheduleDao.GetScheduleForDay(day, cinemaId, cinemaHallId);

            if (schedules == null)
            {
                return new List<Schedule>();
            }

            return schedules;
        }

        public async Task<Schedule> GetScheduleByIdAsync(int scheduleId)
        {
            if (scheduleId < 0)
            {
                throw new ArgumentException("Schedule Id has to be >= 0");
            }

            return await ScheduleDao.FindByIdAsync(scheduleId);
        }

        public async Task<Schedule> UpdateSchedule(Schedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException("Schedule cannot be null");
            }

            if (schedule.Id < 0)
            {
                throw new ArgumentException("Schedule Id has to be >= 0");
            }

            CinemaHall hall = await CinemaHallDao.FindByIdAsync(schedule.CinemaHallId);
            if (hall == null)
            {
                throw new ArgumentException("cinemaHall id does not exist");
            }

            await CheckValidStartTime(schedule, await ScheduleDao.GetScheduleForDay(schedule.StartTime, hall.CinemaId, hall.Id));
            return await ScheduleDao.UpdateAsync(schedule);
        }

        public async Task<IEnumerable<Schedule>> GetScheduleByMovieIdAsync(int movieId, DateTime from, DateTime to)
        {
            if (movieId < 0)
            {
                throw new ArgumentException("Movie Id has to be >= 0");
            }

            return await ScheduleDao.GetScheduleByMovieIdAsync(movieId, from, to);
        }

        private async Task CheckValidStartTime(Schedule schedule, IEnumerable<Schedule> schedulesOfDay)
        {
            Movie scheduleMovie = await MovieDao.FindByIdAsync(schedule.MovieId);

            List<Schedule> found = new List<Schedule>();
            foreach (Schedule daySchedule in schedulesOfDay)
            {
                if (schedule.Id == daySchedule.Id) { continue; }

                Movie dayMovie = await MovieDao.FindByIdAsync(daySchedule.MovieId);
                
                bool oldNotYetEnded = daySchedule.StartTime < schedule.StartTime && daySchedule.StartTime.AddMinutes(dayMovie.DurationMinutes) >= schedule.StartTime;
                bool newNotYetEnded = schedule.StartTime < daySchedule.StartTime && schedule.StartTime.AddMinutes(scheduleMovie.DurationMinutes) >= daySchedule.StartTime;

                if (oldNotYetEnded || newNotYetEnded) 
                {
                    found.Add(daySchedule);
                }
            }

            if (found.Count() != 0)
            {
                throw new ArgumentException($"Timeschedule conlfict with {found.Count()} movies");
            }
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesOfDay(DateTime day, int cinemaId, IEnumerable<Genre> genres)
        {
            if (day == DateTime.MinValue) 
            {
                return new List<Schedule>();
            }
            
            var schedules = await this.GetSchedulesOfDay(day, cinemaId);

            List<Schedule> filtered = new List<Schedule>();

            foreach (var schedule in schedules)
            {
                Movie movie = await MovieDao.FindByIdAsync(schedule.MovieId);
                if (genres.Intersect(movie.Genre).Count() != 0) {
                    filtered.Add(schedule);
                }
            }

            return filtered;
        }
    }
}
