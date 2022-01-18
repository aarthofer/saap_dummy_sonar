using Apollo.BLInterface;
using Apollo.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CinemaController : Controller
    {
        private readonly ICinemaBl cinemaBl;
        private readonly IReservationBl reservationBl;

        public CinemaController(ICinemaBl cinemaBl)
        {
            this.cinemaBl = cinemaBl ?? throw new ArgumentNullException(nameof(cinemaBl));
        }

        [HttpGet("{cinemaId}")]
        public async Task<Cinema> GetAsync([FromRoute] int cinemaId)
        {
            return await cinemaBl.GetCinemaByIdAsync(cinemaId);
        }

        [HttpGet("cinemahall/{cinemaHallId}")]
        public async Task<IEnumerable<CinemaHallSeat>> GetCinehallSeatsAsync([FromRoute] int cinemaHallId)
        {
            return await cinemaBl.GetCinemaHallSeats(cinemaHallId);
        }

        [HttpGet("categories/{cinemaId}")]
        public async Task<IEnumerable<SeatCategory>> GetCategoriesAsync([FromRoute] int cinemaId)
        {
            return await cinemaBl.GetCategoriesForCinemaAsync(cinemaId);
        }

        [HttpPut("category")]
        public async Task<SeatCategory> UpdateCategoryAsync([FromBody] SeatCategory category)
        {
            return await cinemaBl.SaveCategoryAsync(category);
        }

        [HttpPut("{cinemaHallId}")]
        public async Task<IEnumerable<CinemaHallSeat>> UpdateCinemaSeatsAsync([FromRoute] int cinemaHallId, [FromBody] IEnumerable<CinemaHallSeat> seats)
        {
            Console.WriteLine(seats);
            return await cinemaBl.ResetAllSeats(cinemaHallId, seats);
        }
    }
}
