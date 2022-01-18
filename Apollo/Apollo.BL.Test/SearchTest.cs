using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Dal.Mock;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.BL.Test
{
    public class SearchTest
    {
        ISearchBl searchBl;
        MockMovieDao movieMock;
        MockPersonDao personMock;
        MockGenreDao genreMock;

        public SearchTest()
        {
            movieMock = new MockMovieDao();
            personMock = new MockPersonDao();
            genreMock = new MockGenreDao();
            movieMock.Init();
            personMock.Init();
            genreMock.Init();
            searchBl = new SearchBl(movieMock.Object, genreMock.Object, personMock.Object);
        }

        [Theory]
        [InlineData("Movie", "", true, "", "", "", 2)]
        [InlineData("Movie", null, true, null, "", "", 2)]
        [InlineData("1", "", true, "", "", "", 1)]
        [InlineData("", "Crew 5", true, "", "", "", 0)]
        [InlineData("", "Crew 5", false, "", "", "", 1)]
        [InlineData("", "", false, "1", "", "", 1)]
        [InlineData("", "", false, "Genre", "", "", 2)]
        [InlineData(null, null, false, null, "2020-10-01", "2020-10-31", 1)]
        [InlineData(null, null, false, null, "2020-10-01", "", 1)]
        [InlineData(null, null, false, null, "", "2020-10-31", 2)]
        [InlineData(null, null, false, null, "2000-01-01", "2020-12-31", 2)]
        public async Task TestSearch(string title, string person, bool onlyActors, string genre, string from, string to, int result)
        {
            DateTime? fromDt = string.IsNullOrEmpty(from) ? null : (DateTime?)DateTime.Parse(from);
            DateTime? toDt = string.IsNullOrEmpty(to) ? null : (DateTime?)DateTime.Parse(to);

            IEnumerable<Movie> movies = await searchBl.SearchMovies(title, person, onlyActors, genre, fromDt, toDt);
            Assert.Equal(result, movies.Count());
        }

        [Theory]
        [InlineData("Movie", -1, true, -1, "", "", 2)]
        [InlineData("ovi", 0, true, 0, "", "", 2)]
        [InlineData("1", -1, true, -1, "", "", 1)]
        [InlineData("", 5, true, -1, "", "", 0)]
        [InlineData("", 5, false, -1, "", "", 1)]
        [InlineData("", -1, false, 2, "", "", 2)]
        [InlineData("", -1, false, 3, "", "", 1)]
        public async Task TestSearchById(string title, int personId, bool onlyActors, int genreId, string from, string to, int result)
        {
            DateTime? fromDt = string.IsNullOrEmpty(from) ? null : (DateTime?)DateTime.Parse(from);
            DateTime? toDt = string.IsNullOrEmpty(to) ? null : (DateTime?)DateTime.Parse(to);

            IEnumerable<Movie> movies = await searchBl.SearchMovies(title, personId, onlyActors, genreId, fromDt, toDt);
            Assert.Equal(result, movies.Count());
        }

        [Theory]
        [InlineData("2020-12-31", "2000-01-01")]
        public async Task TestSearchInvalid(string from, string to)
        {
            DateTime? fromDt = string.IsNullOrEmpty(from) ? null : (DateTime?)DateTime.Parse(from);
            DateTime? toDt = string.IsNullOrEmpty(to) ? null : (DateTime?)DateTime.Parse(to);

            await Assert.ThrowsAsync<ArgumentException>(() => searchBl.SearchMovies(null, "", false, "", fromDt, toDt));
            await Assert.ThrowsAsync<ArgumentException>(() => searchBl.SearchMovies(null, -1, false, -1, fromDt, toDt));
        }

        [Theory]
        [InlineData("asdf", 0)]
        [InlineData("Person", 3)]
        [InlineData("Person 2", 1)]
        public async Task TestSearchPerson(string name, int count)
        {
            IEnumerable<Person> result = await searchBl.SearchPersonByName(name);
            Assert.Equal(count, result.Count());
        }

        [Theory]
        [InlineData("asdf", 0)]
        [InlineData("Genre", 3)]
        [InlineData("Genre 3", 1)]
        public async Task TestSearchGenre(string name, int count)
        {
            IEnumerable<Genre> result = await searchBl.SearchGenresByName(name);
            Assert.Equal(count, result.Count());
        }

        [Fact]
        public async Task TestSearchPersonGenreInvalid()
        {

            await Assert.ThrowsAsync<ArgumentException>(() => searchBl.SearchGenresByName(""));
            await Assert.ThrowsAsync<ArgumentException>(() => searchBl.SearchGenresByName(null));

            await Assert.ThrowsAsync<ArgumentException>(() => searchBl.SearchPersonByName(""));
            await Assert.ThrowsAsync<ArgumentException>(() => searchBl.SearchPersonByName(null));
        }
    }
}
