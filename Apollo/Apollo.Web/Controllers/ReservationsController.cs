using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apollo.BLInterface;
using Apollo.Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Apollo.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationsController : Controller {

        public IReservationBl ReservationBl { get; }

        public ReservationsController(IReservationBl reservationBl)
        {
            ReservationBl = reservationBl;
        }

        [HttpGet("{scheduleId}")]
        public async Task<ActionResult<IEnumerable<ReservationSeat>>> GetReservationsForSchedule(int scheduleId)
        {
            if (scheduleId < 0)
            {
                return BadRequest();
            }

            return (await ReservationBl.GetSeatsAsync(scheduleId)).ToList();
        }

    }
}