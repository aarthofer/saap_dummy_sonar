using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IMovieDao : IApolloDao<Movie>
    {
        Task<IEnumerable<Movie>> SearchMovies(string title, string personName, bool onlyActors, string genreName, DateTime? from, DateTime? to);
        Task<IEnumerable<Movie>> SearchMovies(string title, int? personId, bool onlyActors, int? genreId, DateTime? from, DateTime? to);
    }
}
