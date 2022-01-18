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
    [Route("[Controller]")]
    public class GenreController : Controller
    {
        private readonly IMovieBl movieBl;

        public GenreController(IMovieBl movieBl)
        {
            this.movieBl = movieBl ?? throw new ArgumentNullException(nameof(movieBl));
        }

        [HttpGet]
        public async Task<IEnumerable<Genre>> GetGenresAsync()
        {
            return await movieBl.GetGenresAsync();
        }
    }
}
