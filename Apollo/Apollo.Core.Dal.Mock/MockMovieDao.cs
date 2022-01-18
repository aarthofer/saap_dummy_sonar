using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    public class MockMovieDao : Mock<IMovieDao>
    {
        private List<Movie> movies;

        public void Init()
        {
            CreateDummyMovies();

            this.MockCreateAsync()
                .MockDeleteByIdAsync()
                .MockFindAllAsync()
                .MockFindByIdAsync()
                .MockUpdateAsync()
                .MockSearchByIds()
                .MockSearchByNames();
        }

        public MockMovieDao MockCreateAsync()
        {
            Setup(m => m.CreateAsync(It.IsAny<Movie>()))
                .Returns((Movie m) =>
                {
                    m.Id = 99;
                    movies.Add(m);
                    return Task.FromResult(m);
                });
            return this;
        }

        public MockMovieDao MockDeleteByIdAsync()
        {
            Setup(m => m.DeleteByIdAsync(It.IsAny<int>()))
                .Returns((int input) =>
                {
                    var m = movies.FirstOrDefault(x => x.Id == input);
                    if (m != null)
                    {
                        movies.Remove(m);
                        return Task.FromResult(1);
                    }
                    else
                    {
                        return Task.FromResult(0);
                    }
                });
            return this;
        }

        public MockMovieDao MockSearchByIds()
        {
            Setup(x => x.SearchMovies(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                  .Returns((string title, int personId, bool onlyActors, int genreId, DateTime? from, DateTime? to) =>
                    Task.FromResult(
                        movies.Where(x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                        .Where(x => genreId <= 0 ? true : x.Genre.Any(y => y.Id == genreId))
                        .Where(x => personId <= 0 ? true : onlyActors ? x.Actors.Any(y => y.Id == personId) : x.Actors.Any(y => y.Id == personId) || x.Crew.Any(y => y.Key.Id == personId))
                        .Where(x => from == null ? true : x.ReleaseDate >= from)
                        .Where(x => to == null ? true : x.ReleaseDate <= to)
                        .Select(x => x))
                  );
            return this;
        }

        public MockMovieDao MockSearchByNames()
        {
            Setup(x => x.SearchMovies(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                  .Returns((string title, string person, bool onlyActors, string genre, DateTime? from, DateTime? to) =>
                    Task.FromResult(
                        movies.Where(x => title == null ? true : x.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                        .Where(x => genre == null ? true : x.Genre.Any(y => y.Name.Contains(genre, StringComparison.OrdinalIgnoreCase)))
                        .Where(x => person == null ? true : onlyActors ? x.Actors.Any(y => y.Name.Contains(person, StringComparison.OrdinalIgnoreCase)) : x.Actors.Any(y => y.Name.Contains(person, StringComparison.OrdinalIgnoreCase)) || x.Crew.Any(y => y.Key.Name.Contains(person, StringComparison.OrdinalIgnoreCase)))
                        .Where(x => from == null ? true : x.ReleaseDate >= from)
                        .Where(x => to == null ? true : x.ReleaseDate <= to)
                        .Select(x => x))
                  );
            return this;
        }

        public MockMovieDao MockFindAllAsync()
        {
            Setup(m => m.FindAllAsync())
                .Returns(Task.FromResult(movies.AsEnumerable()));
            return this;
        }

        public MockMovieDao MockFindByIdAsync()
        {
            Setup(m => m.FindByIdAsync(It.IsAny<int>()))
                .Returns((int input) =>
                {
                    return Task.FromResult(movies.FirstOrDefault(x => x.Id == input));
                });
            return this;
        }

        public MockMovieDao MockUpdateAsync()
        {
            Setup(m => m.UpdateAsync(It.IsAny<Movie>()))
                .Returns((Movie input) =>
                {
                    if (movies.Any(x => x.Id == input.Id))
                    {
                        movies.Remove(movies.FirstOrDefault(x => x.Id == input.Id));
                        movies.Add(input);
                        return Task.FromResult(input);
                    }
                    else
                    {
                        return Task.FromResult<Movie>(null);
                    }
                });
            return this;
        }

        private void CreateDummyMovies()
        {
            movies = new List<Movie>() {
                new Movie()
                {
                    Id = 1,
                    Title = "Movie 1",
                    Description ="Description 1",
                    DurationMinutes = 100,
                    Genre = GetGenres(1,2),
                    Image = "Image",
                    ReleaseDate = new DateTime(2020,10,10),
                    Actors = GetActors(1,2),
                    Crew = GetCrew(1,2)
                },
                new Movie()
                {
                    Id = 2,
                    Title = "Movie 2",
                    Description ="Description 2",
                    DurationMinutes = 100,
                    Genre = GetGenres(2, 3),
                    Image = "Image",
                    ReleaseDate = new DateTime(2019,10,10),
                    Actors = GetActors(3,4),
                    Crew = GetCrew(3,4,5)
                }
            };
        }

        private IEnumerable<KeyValuePair<Person, string>> GetCrew(params int[] ids)
        {
            return new List<KeyValuePair<Person, string>>()
            {
                new KeyValuePair<Person, string>(new Person()
                {
                    Id = 5,
                    Name = "Crew 5"
                }, "Role 5"),
                new KeyValuePair<Person, string>(new Person()
                {
                    Id = 6,
                    Name = "Crew 6"
                }, "Role 6"),
                new KeyValuePair<Person, string>(new Person()
                {
                    Id = 7,
                    Name = "Crew 7"
                }, "Role 7"),
                new KeyValuePair<Person, string>(new Person()
                {
                    Id = 8,
                    Name = "Crew 8"
                }, "Role 8")
            }.Where(x => ids.Contains(x.Key.Id));
        }

        private IEnumerable<Person> GetActors(params int[] ids)
        {
            return new List<Person>()
            {
                new Person()
                {
                    Id = 1,
                    Name = "Person 1"
                },
                new Person()
                {
                    Id = 2,
                    Name = "Person 2"
                },
                new Person()
                {
                    Id = 3,
                    Name = "Person 3"
                },
                new Person()
                {
                    Id = 4,
                    Name = "Person 4"
                }
            }.Where(x => ids.Contains(x.Id));
        }

        private IEnumerable<Genre> GetGenres(params int[] ids)
        {
            return new List<Genre>()
            {
                new Genre()
                {
                    Id = 1,
                    Name = "Genre 1"
                },
                new Genre()
                {
                    Id = 2,
                    Name = "Genre 2"
                },
                new Genre()
                {
                    Id = 3,
                    Name = "Genre 3"
                },
                new Genre()
                {
                    Id = 4,
                    Name = "Genre 5"
                }
            }.Where(x => ids.Contains(x.Id));
        }
    }
}
