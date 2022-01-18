using Apollo.BLInterface;
using Apollo.Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CinemasController : Controller
    {
        public ICinemaBl CinemaBl { get; }

        public CinemasController(ICinemaBl cinemaBl)
        {
            CinemaBl = cinemaBl;
        }

        [HttpGet]
        public async Task<IEnumerable<Cinema>> GetAllCinemasAsync()
        {
            var cinemas = new List<Cinema>();
            cinemas.Add(await CinemaBl.GetCinemaByIdAsync(1));
            return cinemas;
        }

        [HttpGet("{cinemaId}")]
        public async Task<Cinema> GetCinemaById(int cinemaId)
        {
            return await CinemaBl.GetCinemaByIdAsync(cinemaId);
        }

        [HttpGet("{cinemaId}/SeatCategories")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<SeatCategory>>> GetCategoriesForCinema(int cinemaId)
        {
            var categories = (await CinemaBl.GetCategoriesForCinemaAsync(cinemaId)).ToList();

            if (categories.Count() == 0)
            {
                return NotFound();
            }

            return  categories;
        }
        
        [HttpGet("{cinemaId}/SeatCategories/{seatCategoryId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SeatCategory>> GetSeatCategoryById(int cinemaId, int seatCategoryId)
        {
            if (cinemaId < 0 || seatCategoryId < 0) { return BadRequest(); }

            try
            {
                var category = await CinemaBl.GetCategoryByIdAsync(seatCategoryId);

                if (category.CinemaId != cinemaId)
                {
                    return BadRequest();
                }

                return category;
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPost("{cinemaId}/SeatCategories")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<SeatCategory>> AddSeatCategory(int cinemaId, SeatCategory category)
        {
            try
            {

                if (cinemaId < 0 || category == null || category.Id > 0) { return BadRequest();  }
                
                category.CinemaId = cinemaId;
                var newCategory = await CinemaBl.SaveCategoryAsync(category);

                return CreatedAtAction(
                    actionName: nameof(GetSeatCategoryById),
                    routeValues: new { cinemaId = cinemaId, seatCategoryId = newCategory.Id },
                    value: newCategory);
                
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }

        [HttpPut("{cinemaId}/SeatCategories/{seatCategoryId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SeatCategory>> UpdateSeatCategory(int cinemaId, int seatCategoryId, [FromBody] SeatCategory category)
        {
            if (cinemaId < 0 || seatCategoryId < 0 || category == null) { return BadRequest(); }
            try
            {
                category.CinemaId = cinemaId;
                category.Id = seatCategoryId;

                return await CinemaBl.SaveCategoryAsync(category);
            
            } catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }

        [HttpDelete("{cinemaId}/SeatCategories/{SeatCategoryId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> DeleteSeatCategory(int cinemaId, int seatCategoryId)
        {            
            if (cinemaId < 0 || seatCategoryId < 0) { return BadRequest(); }
            try
            {
                return await CinemaBl.DeleteCategoryByIdAsync(seatCategoryId);
            
            } catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Cinema>> AddCinema(Cinema cinema)
        {
            try
            {
                Cinema newCinema = await CinemaBl.CreateCinemaAsync(cinema);
                return CreatedAtAction(actionName: nameof(GetCinemaById),
                    routeValues: new { cinemaId = newCinema.Id },
                    value: newCinema);
                    
            } 
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPut("{cinemaId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Cinema>> UpdateCinema(int cinemaId, Cinema cinema)
        {
            if (cinemaId != cinema.Id)
            {
                return BadRequest();
            }

            try
            {
                return await CinemaBl.UpdateCinemaAsync(cinema);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }



    }
}
