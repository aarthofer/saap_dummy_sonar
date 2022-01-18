using Apollo.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IGenreDao : IApolloDao<Genre>
    {
        Task<IEnumerable<Genre>> FindGenresByMovieIdAsync(int id);

        Task<Genre> GetOrAddGenreByNameAsync(string name);
        Task AddGenreToMovieAsync(Movie dbMovie, IEnumerable<Genre> genres);
        Task RemoveGenreFromMovieAsync(Movie m1, IList<Genre> dbGenres);
        Task<IEnumerable<Genre>> FindGenresByNameAsync(string name);
    }
}
