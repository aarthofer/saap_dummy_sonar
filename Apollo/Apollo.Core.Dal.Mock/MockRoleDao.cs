using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    // https://exceptionnotfound.net/using-moq-to-create-fluent-test-classes-in-asp-net-core/
    public class MockRoleDao : Mock<IRoleDao>
    {
        private List<Role> roles = new List<Role>()
        {
            new Role(){ Id = 1, Name= "Role 1"},
            new Role(){ Id = 2, Name= "Role 2"},
            new Role(){ Id = 3, Name= "Role 3"}
        };
        public MockRoleDao MockCreateAsync()
        {
            Setup(x => x.CreateAsync(It.IsAny<Role>()))
                .Returns(Task.FromResult(new Role()
                {
                    Id = 1,
                    Name = "TestRole"
                }));
            return this;
        }

        public MockRoleDao MockDeleteByIdAsync()
        {
            Setup(x => x.DeleteByIdAsync(It.IsAny<int>())).Returns((int input) =>
             {
                 var r = roles.FirstOrDefault(x => x.Id == input);
                 if (r != null)
                 {
                     roles.Remove(r);
                     return Task.FromResult(1);
                 }
                 else
                 {
                     return Task.FromResult(0);
                 }
             });
            return this;
        }

        public MockRoleDao MockFindAllAsync()
        {
            Setup(x => x.FindAllAsync())
                   .Returns(Task.FromResult(roles.AsEnumerable()));
            return this;
        }

        public MockRoleDao MockFindByIdAsync()
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                  .Returns((int input) =>
                    Task.FromResult(roles.FirstOrDefault(x => x.Id == input))
                  );
            return this;
        }

        public MockRoleDao MockGetOrAddRoleByNameAsync()
        {
            Setup(x => x.GetOrAddRoleByNameAsync(It.IsAny<string>())).Returns((string input) =>
            {
                var r = roles.FirstOrDefault(x => x.Name == input);
                if (r != null)
                {
                    return Task.FromResult(r);
                }
                else
                {
                    Role r1 = new Role()
                    {
                        Id = roles.Max(r => r.Id) + 1,
                        Name = input
                    };
                    roles.Add(r1);
                    return Task.FromResult(r1);
                }
            });
            return this;
        }

        public MockRoleDao MockUpdateAsync()
        {
            Setup(r => r.UpdateAsync(It.IsAny<Role>()))
                .Returns((Role input) =>
                {
                    if (roles.Any(x => x.Id == input.Id))
                    {
                        roles.Remove(roles.FirstOrDefault(x => x.Id == input.Id));
                        roles.Add(input);
                        return Task.FromResult(input);
                    }
                    else
                    {
                        return Task.FromResult<Role>(null);
                    }
                });
            return this;
        }
    }
}
