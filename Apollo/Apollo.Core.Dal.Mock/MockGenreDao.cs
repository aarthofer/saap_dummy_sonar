using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    // https://exceptionnotfound.net/using-moq-to-create-fluent-test-classes-in-asp-net-core/
    public class MockGenreDao : Mock<IGenreDao>
    {
        private List<Genre> genres = new List<Genre>()
        {
            new Genre(){ Id = 1, Name= "Genre 1"},
            new Genre(){ Id = 2, Name= "Genre 2"},
            new Genre(){ Id = 3, Name= "Genre 3"}
        };
        public MockGenreDao MockCreateAsync()
        {
            Setup(x => x.CreateAsync(It.IsAny<Genre>()))
                .Returns(Task.FromResult(new Genre()
                {
                    Id = 1,
                    Name = "TestGenre"
                }));
            return this;
        }

        public void Init()
        {
            MockCreateAsync()
                .MockFindAllAsync()
                .MockFindByIdAsync()
                .MockUpdateAsync()
                .MockGetOrAddGenreByNameAsync()
                .MockDeleteByIdAsync()
                .MockFindGenreByName();
        }

        public MockGenreDao MockFindGenreByName()
        {
            Setup(x => x.FindGenresByNameAsync(It.IsAny<string>()))
                  .Returns((string input) =>
                    Task.FromResult(genres.Where(x => x.Name.Contains(input, StringComparison.OrdinalIgnoreCase)).Select(x => x))
                  );
            return this;
        }

        public MockGenreDao MockDeleteByIdAsync()
        {
            Setup(x => x.DeleteByIdAsync(It.IsAny<int>())).Returns((int input) =>
             {
                 var r = genres.FirstOrDefault(x => x.Id == input);
                 if (r != null)
                 {
                     genres.Remove(r);
                     return Task.FromResult(1);
                 }
                 else
                 {
                     return Task.FromResult(0);
                 }
             });
            return this;
        }

        public MockGenreDao MockFindAllAsync()
        {
            Setup(x => x.FindAllAsync())
                   .Returns(Task.FromResult(genres.AsEnumerable()));
            return this;
        }

        public MockGenreDao MockFindByIdAsync()
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                  .Returns((int input) =>
                    Task.FromResult(genres.FirstOrDefault(x => x.Id == input))
                  );
            return this;
        }

        public MockGenreDao MockGetOrAddGenreByNameAsync()
        {
            Setup(x => x.GetOrAddGenreByNameAsync(It.IsAny<string>())).Returns((string input) =>
            {
                var r = genres.FirstOrDefault(x => x.Name == input);
                if (r != null)
                {
                    return Task.FromResult(r);
                }
                else
                {
                    Genre r1 = new Genre()
                    {
                        Id = genres.Max(r => r.Id) + 1,
                        Name = input
                    };
                    genres.Add(r1);
                    return Task.FromResult(r1);
                }
            });
            return this;
        }

        public MockGenreDao MockUpdateAsync()
        {
            Setup(r => r.UpdateAsync(It.IsAny<Genre>()))
                .Returns((Genre input) =>
                {
                    if (genres.Any(x => x.Id == input.Id))
                    {
                        genres.Remove(genres.FirstOrDefault(x => x.Id == input.Id));
                        genres.Add(input);
                        return Task.FromResult(input);
                    }
                    else
                    {
                        return Task.FromResult<Genre>(null);
                    }
                });
            return this;
        }
    }
}
