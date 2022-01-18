using Apollo.BLInterface;
using Apollo.Core.Dal.Dao;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class MovieBl : IMovieBl
    {
        private readonly IMovieDao movieDao;
        private readonly IGenreDao genreDao;
        private readonly IRoleDao roleDao;

        public MovieBl(IMovieDao movieDao, IGenreDao genreDao, IRoleDao roleDao)
        {
            this.movieDao = movieDao ?? throw new ArgumentNullException(nameof(movieDao));
            this.genreDao = genreDao ?? throw new ArgumentNullException(nameof(genreDao));
            this.roleDao = roleDao ?? throw new ArgumentNullException(nameof(roleDao));
        }

        public async Task<Movie> GetMovieByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }
            return await movieDao.FindByIdAsync(id);
        }

        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            if (movie == null)
            {
                throw new ArgumentNullException(nameof(movie));
            }
            return await movieDao.CreateAsync(movie);
        }

        public async Task<Movie> UpdateMovieAsync(Movie movie)
        {
            if (movie == null)
            {
                throw new ArgumentNullException(nameof(movie));
            }

            if (movie.Id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }

            return await movieDao.UpdateAsync(movie);
        }

        public async Task<int> DeleteMovieByIdAsync(int id)
        {
            return await movieDao.DeleteByIdAsync(id);
        }

        public async Task<Genre> AddGenreAsync(Genre genre)
        {
            if (genre == null)
            {
                throw new ArgumentNullException(nameof(genre));
            }

            return await this.genreDao.GetOrAddGenreByNameAsync(genre.Name);
        }

        public async Task<Genre> GetGenreByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }
            return await this.genreDao.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Genre>> GetGenresAsync()
        {
            return await this.genreDao.FindAllAsync();
        }

        public async Task<int> RemoveGenreAsync(int id)
        {
            return await this.genreDao.DeleteByIdAsync(id);
        }

        public async Task<Role> AddRoleAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return await this.roleDao.GetOrAddRoleByNameAsync(role.Name);
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }

            return await this.roleDao.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Role>> GetRolesAsync()
        {
            return await this.roleDao.FindAllAsync();
        }

        public async Task<int> RemoveRoleAsync(int id)
        {
            return await this.roleDao.DeleteByIdAsync(id);
        }

        public async Task<Genre> UpdateGenreAsync(Genre genre)
        {
            if (genre == null)
            {
                throw new ArgumentNullException(nameof(genre));
            }

            if (genre.Id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }

            return await genreDao.UpdateAsync(genre);
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (role.Id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }

            return await roleDao.UpdateAsync(role);
        }
    }
}
