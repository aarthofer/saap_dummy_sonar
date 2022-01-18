using Apollo.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IMovieBl
    {
        Task<Movie> GetMovieByIdAsync(int id);

        Task<Movie> CreateMovieAsync(Movie movie);

        Task<Movie> UpdateMovieAsync(Movie movie);

        Task<int> DeleteMovieByIdAsync(int id);

        Task<Genre> GetGenreByIdAsync(int id);
        Task<IEnumerable<Genre>> GetGenresAsync();

        Task<Genre> AddGenreAsync(Genre genre);

        Task<Genre> UpdateGenreAsync(Genre genre);

        Task<int> RemoveGenreAsync(int id);

        Task<Role> GetRoleByIdAsync(int id);
        Task<IEnumerable<Role>> GetRolesAsync();

        Task<Role> AddRoleAsync(Role role);

        Task<Role> UpdateRoleAsync(Role role);

        Task<int> RemoveRoleAsync(int id);
    }
}
