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
    public class MovieController : Controller
    {
        private readonly ILogger<MovieController> logger;
        private readonly IMovieBl movieBl;
        private readonly ISearchBl searchBl;

        public MovieController(ILogger<MovieController> logger, IMovieBl movieBl, ISearchBl searchBl)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.movieBl = movieBl ?? throw new ArgumentNullException(nameof(movieBl));
            this.searchBl = searchBl ?? throw new ArgumentNullException(nameof(searchBl));
        }

        [HttpGet("{movieId}")]
        public async Task<Movie> GetAsync([FromRoute] int movieId)
        {
            return await movieBl.GetMovieByIdAsync(movieId);
        }

        [HttpPost]
        public async Task<Movie> StoreMovieAsync([FromBody] Movie movie)
        {
            if (movie == null)
            {
                throw new System.Web.Http.HttpResponseException(new System.Net.Http.HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ReasonPhrase = "Post data are null"
                });
            }

            if (movie.Id <= 0)
            {
                return await movieBl.CreateMovieAsync(movie);
            }
            else
            {
                return await movieBl.UpdateMovieAsync(movie);
            }
        }

        [HttpGet("search")]
        public async Task<IEnumerable<Movie>> SearchAsync([FromQuery] string title, [FromQuery] string person, [FromQuery] bool onlyActors, [FromQuery] string genre)
        {
            return await searchBl.SearchMovies(title, person, onlyActors, genre, null, null);
        }
    }
}
