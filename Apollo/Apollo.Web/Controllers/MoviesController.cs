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
    public class MoviesController : Controller
    {
        private readonly ILogger<MoviesController> logger;
        private readonly IMovieBl movieBl;
        private readonly ISearchBl searchBl;
        private readonly IPersonBl personBl;

        public MoviesController(ILogger<MoviesController> logger, IMovieBl movieBl, ISearchBl searchBl, IPersonBl personBl)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.movieBl = movieBl ?? throw new ArgumentNullException(nameof(movieBl));
            this.searchBl = searchBl ?? throw new ArgumentNullException(nameof(searchBl));
            this.personBl = personBl ?? throw new ArgumentNullException(nameof(personBl));
        }

        [HttpGet("{movieId}")]
        public async Task<ActionResult<Movie>> GetByIdAsync([FromRoute] int movieId)
        {
            try
            {
                var movie = await movieBl.GetMovieByIdAsync(movieId);

                if (movie == null)
                {
                    return NotFound();
                }

                return movie;
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPut("{movieId}")]
        public async Task<ActionResult<Movie>> UpdateMovie(int movieId, Movie movie)
        {
            if (movieId < 0 || movie.Id != movieId) { return BadRequest(); }

            try
            {
                return await movieBl.UpdateMovieAsync(movie);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpPost("{movieId}")]
        public async Task<ActionResult<Movie>> CreateMovie(int movieId, Movie movie)
        {
            if (movieId < 0 || movie.Id != movieId) { return BadRequest(); }

            try
            {
                var newMovie = await movieBl.UpdateMovieAsync(movie);

                return CreatedAtAction(actionName: nameof(GetByIdAsync),
                    routeValues: new { movieId = newMovie.Id },
                    value: newMovie
                    );
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpGet("actors")]
        public async Task<IEnumerable<Person>> GetAllActors()
        {
            return await personBl.GetPersons();  
        }

        [HttpGet("search")]
        public async Task<IEnumerable<Movie>> SearchAsync([FromQuery] string title)
        {
            return await searchBl.SearchMovies(title, "", true, "", null, null);
        }

        [HttpGet("genres")]
        public async Task<IEnumerable<Genre>> GetAllGenres()
        {
            return await movieBl.GetGenresAsync();
        }
    }
}
