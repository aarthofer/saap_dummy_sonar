using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    public class MockUserDao : Mock<IUserDao>
    {
        public MockUserDao MockFindById(User user)
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(user));

            return this;
        }

        public MockUserDao MockFindByInvalidID()
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<User>(null));
            return this;
        }
    }
}
