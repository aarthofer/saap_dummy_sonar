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
    public class SchedulesController : Controller
    {
        private readonly ILogger<SchedulesController> logger;
        private readonly IScheduleBl scheduleBl;

        public SchedulesController(ILogger<SchedulesController> logger, IScheduleBl scheduleBl)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.scheduleBl = scheduleBl ?? throw new ArgumentNullException(nameof(scheduleBl));
        }

        [HttpGet("cinemas/{cinemaId}/filtered")]
        public async Task<ActionResult<List<Schedule>>> GetFilteredSchedules(int cinemaId, [FromQuery] DateTime? day)
        {
            if (day == null)
            {
                return BadRequest();
            }

            return (await scheduleBl.GetSchedulesOfDay(day ?? DateTime.Now, cinemaId)).ToList();
        }

        [HttpGet("cinemaHalls/{cinemaHallId}")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedulesOfDayOfCinemaHallAsync(int cinemaHallId, [FromQuery] DateTime? day)
        {
            if (day == null)
            {
                return BadRequest();
            }

            return (await scheduleBl.GetSchedulesOfDay(day ?? DateTime.Now, cinemaHallId)).ToList();
        }

        [HttpGet("{scheduleId}")]
        public async Task<ActionResult<Schedule>> GetScheduleById(int scheduleId)
        {
            try
            {
                return await scheduleBl.GetScheduleByIdAsync(scheduleId);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpGet("cinemas/{cinemaId}")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedulesOfDay(int cinemaId, [FromQuery] DateTime? day)
        {
            if(day == null)
            {
                return BadRequest();
            }

            var schedules =  await scheduleBl.GetSchedulesOfDay(day ?? DateTime.Now, cinemaId);
            return schedules.ToList();
        }

        [HttpPost("cinemas/{cinemaId}")]
        public async Task<ActionResult<Schedule>> AddSchedule(int cinemaId, Schedule schedule)
        {
            try
            {
                var newSchedule = await scheduleBl.CreateScheduleAsync(cinemaId, schedule);

                return CreatedAtAction(
                    actionName: nameof(GetScheduleById),
                    routeValues: new { scheduleId = newSchedule.Id},
                    value: newSchedule);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPut("cinemas")]
        public async Task<ActionResult<Schedule>> UpdateSchedule(Schedule schedule)
        {
            try 
            {
                return await scheduleBl.UpdateSchedule(schedule);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }
    }
}