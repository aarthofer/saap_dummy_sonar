using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface ISearchBl
    {
        Task<IEnumerable<Movie>> SearchMovies(string title, string person, bool onlyActors, string genre, DateTime? from, DateTime? to);
        Task<IEnumerable<Movie>> SearchMovies(string title, int? person, bool onlyActors, int? genre, DateTime? from, DateTime? to);
        Task<IEnumerable<Genre>> SearchGenresByName(string name);
        Task<IEnumerable<Person>> SearchPersonByName(string name);
    }
}
