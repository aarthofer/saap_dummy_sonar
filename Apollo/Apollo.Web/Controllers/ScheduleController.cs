using Apollo.BLInterface;
using Apollo.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScheduleController : Controller
    {
        private const int DEFAULT_SCHEDULE_SEARCH_DAYS = 14;
        private readonly ILogger<ScheduleController> logger;
        private readonly IScheduleBl scheduleBl;
        private readonly IReservationBl reservationBl;

        public ScheduleController(ILogger<ScheduleController> logger, IScheduleBl scheduleBl, IReservationBl reservationBl)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.scheduleBl = scheduleBl ?? throw new ArgumentNullException(nameof(scheduleBl));
            this.reservationBl = reservationBl ?? throw new ArgumentNullException(nameof(reservationBl));
        }

        [HttpGet]
        public async Task<IEnumerable<Schedule>> GetAsync([FromQuery] DateTime? day)
        {
            if (day == null)
            {
                return new List<Schedule>();
            }
            var schedules = await scheduleBl.GetSchedulesOfDay(day.Value, 1);
            return schedules;
        }

        [HttpPost]
        public async Task<Schedule> StoreSchedule([FromBody] Schedule schedule)
        {

            if (schedule == null)
            {
                throw new System.Web.Http.HttpResponseException(new System.Net.Http.HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ReasonPhrase = "Post data are null"
                });
            }
            if (schedule.Id < 0)
            {
                return await scheduleBl.CreateScheduleAsync(1, schedule);
            }
            else
            {
                return await scheduleBl.UpdateSchedule(schedule);
            }
        }

        [HttpDelete("{scheduleId}")]
        public async Task DeleteSchedule([FromRoute] int scheduleId)
        {
            if (scheduleId <= 0)
            {
                throw new System.Web.Http.HttpResponseException(new System.Net.Http.HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ReasonPhrase = "No scheduleId"
                });
            }
            await scheduleBl.DeleteSchedule(scheduleId);
        }

        [HttpGet("movieid/{movieid}")]
        public async Task<IEnumerable<Schedule>> GetSchedulesByMovieIdAsync([FromRoute] int movieid, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (movieid < 0)
            {
                return new List<Schedule>();
            }

            if (to == null)
            {
                to = DateTime.Now.AddDays(DEFAULT_SCHEDULE_SEARCH_DAYS);
            }
            if (from == null)
            {
                from = DateTime.Now;
            }

            var schedules = await scheduleBl.GetScheduleByMovieIdAsync(movieid, (DateTime)from, (DateTime)to);
            return schedules;
        }

        [HttpGet("reservationstate/{scheduleId}")]
        public async Task<ReservationState> GetReserverationStae([FromRoute] int scheduleId)
        {
            IEnumerable<ReservationSeat> seats = await reservationBl.GetSeatsAsync(scheduleId);
            int free = seats.Where(s => s.state == ReservationSeat.State.free).Count();
            int total = seats.Where(s => s.state == ReservationSeat.State.free || s.state == ReservationSeat.State.reserved).Count();
            return new ReservationState { Free = free, Total = total };
        }
    }

    public class ReservationState
    {
        public int Free { get; set; }
        public int Total { get; set; }
    }
}
