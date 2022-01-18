using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class SearchBl : ISearchBl
    {
        private readonly IMovieDao movieDao;
        private readonly IGenreDao genreDao;
        private readonly IPersonDao personDao;

        public SearchBl(IMovieDao movieDao, IGenreDao genreDao, IPersonDao personDao)
        {
            this.movieDao = movieDao ?? throw new ArgumentNullException(nameof(movieDao));
            this.genreDao = genreDao ?? throw new ArgumentNullException(nameof(genreDao));
            this.personDao = personDao ?? throw new ArgumentNullException(nameof(personDao));
        }

        public async Task<IEnumerable<Genre>> SearchGenresByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            return await genreDao.FindGenresByNameAsync(name);
        }

        public async Task<IEnumerable<Person>> SearchPersonByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            return await personDao.FindPersonsByNameAsync(name);
        }

        public async Task<IEnumerable<Movie>> SearchMovies(string title, string person, bool onlyActors, string genre, DateTime? from, DateTime? to)
        {
            if (from != null && to != null && from > to)
            {
                throw new ArgumentException($"'{nameof(to)}' cannot be before {nameof(from)}");
            }
            return await movieDao.SearchMovies(title, person, onlyActors, genre, from, to);
        }

        public async Task<IEnumerable<Movie>> SearchMovies(string title, int? person, bool onlyActors, int? genre, DateTime? from, DateTime? to)
        {
            if (from != null && to != null && from > to)
            {
                throw new ArgumentException($"'{nameof(to)}' cannot be before {nameof(from)}");
            }
            return await movieDao.SearchMovies(title, person, onlyActors, genre, from, to);
        }
    }
}
