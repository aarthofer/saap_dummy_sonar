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
    public class PersonController : Controller
    {
        private readonly ISearchBl searchBl;

        public PersonController(ISearchBl searchBl)
        {
            this.searchBl = searchBl ?? throw new ArgumentNullException(nameof(searchBl));
        }

        [HttpGet("{name}")]
        public async Task<IEnumerable<Person>> SearchPersonAsync([FromRoute] string name)
        {
            return await searchBl.SearchPersonByName(name);
        }
    }
}
