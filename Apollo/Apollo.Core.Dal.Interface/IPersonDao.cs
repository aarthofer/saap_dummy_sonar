using Apollo.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Interface
{
    public interface IPersonDao : IApolloDao<Person>
    {
        Task<IEnumerable<Person>> FindPersonsByNameAsync(string name);
        Task<IEnumerable<Person>> FindActorsByMovieIdAsync(int id);
        Task<IEnumerable<KeyValuePair<Person, string>>> FindCrewByMovieIdAsync(int id);
        Task AddActorsToMovieAsync(Movie dbMovie, IEnumerable<Person> actors);
        Task AddCrewToMovieAsync(Movie dbMovie, IEnumerable<KeyValuePair<Person, string>> crew);
        Task<Person> GetOrAddPersonByNameAsync(string name);
        Task RemoveActorsFromMovieAsync(Movie m, List<Person> lists);
        Task RemoveCrewFromMovieAsync(Movie m, List<KeyValuePair<Person, string>> lists);
    }
}
