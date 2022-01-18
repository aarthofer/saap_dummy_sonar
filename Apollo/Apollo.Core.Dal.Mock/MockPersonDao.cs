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
    public class MockPersonDao : Mock<IPersonDao>
    {
        private List<Person> genres = new List<Person>()
        {
            new Person(){ Id = 1, Name= "Person 1"},
            new Person(){ Id = 2, Name= "Person 2"},
            new Person(){ Id = 3, Name= "Person 3"}
        };

        public void Init()
        {
            MockFindAllAsync()
            .MockFindByIdAsync()
            .MockUpdateAsync()
            .MockGetOrAddPersonByNameAsync()
            .MockDeleteByIdAsync()
            .MockFindPersonsByName();
        }

        public MockPersonDao MockFindPersonsByName()
        {
            Setup(x => x.FindPersonsByNameAsync(It.IsAny<string>()))
                  .Returns((string input) =>
                    Task.FromResult(genres.Where(x => x.Name.Contains(input, StringComparison.OrdinalIgnoreCase)).Select(x => x))
                  );
            return this;
        }

        public MockPersonDao MockDeleteByIdAsync()
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

        public MockPersonDao MockFindAllAsync()
        {
            Setup(x => x.FindAllAsync())
                   .Returns(Task.FromResult(genres.AsEnumerable()));
            return this;
        }

        public MockPersonDao MockFindByIdAsync()
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                  .Returns((int input) =>
                    Task.FromResult(genres.FirstOrDefault(x => x.Id == input))
                  );
            return this;
        }

        public MockPersonDao MockGetOrAddPersonByNameAsync()
        {
            Setup(x => x.GetOrAddPersonByNameAsync(It.IsAny<string>())).Returns((string input) =>
            {
                var r = genres.FirstOrDefault(x => x.Name == input);
                if (r != null)
                {
                    return Task.FromResult(r);
                }
                else
                {
                    Person r1 = new Person()
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

        public MockPersonDao MockUpdateAsync()
        {
            Setup(r => r.UpdateAsync(It.IsAny<Person>()))
                .Returns((Person input) =>
                {
                    if (genres.Any(x => x.Id == input.Id))
                    {
                        genres.Remove(genres.FirstOrDefault(x => x.Id == input.Id));
                        genres.Add(input);
                        return Task.FromResult(input);
                    }
                    else
                    {
                        return Task.FromResult<Person>(null);
                    }
                });
            return this;
        }
    }
}
