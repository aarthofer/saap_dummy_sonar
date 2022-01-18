using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Core.Dal.Mock
{
    public class MockCinemaDao : Mock<ICinemaDao>
    {
        public MockCinemaDao MockGetById(Cinema cinema)
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                   .Returns(Task.FromResult(cinema));
            return this;
        }

        public MockCinemaDao MockGetByInvalidId()
        {
            Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult<Cinema>(null));

            return this;
        }

        public MockCinemaDao MockCreateCinema(Cinema cinema)
        {
            Setup(x => x.CreateAsync(It.IsAny<Cinema>()))
                .Returns(Task.FromResult(cinema));
            return this;
        }

        public MockCinemaDao MockFindCinema(Cinema cinema)
        {
            Setup(x => x.FindCinemaByName(It.IsAny<string>()))
                .Returns(Task.FromResult(cinema));
            return this;
        }

        public MockCinemaDao MockRemoveCinema()
        {
            Setup(x => x.RemoveCinema(It.IsAny<Cinema>()))
                .Returns(Task.FromResult(1));

            return this;
        }

        public MockCinemaDao MockUpdateCinema(Cinema cinema)
        {
            Setup(x => x.UpdateAsync(It.IsAny<Cinema>()))
                .Returns(Task.FromResult(cinema));
            return this;
        }

        public MockCinemaDao MockSaveCategory(SeatCategory category)
        {
            Setup(x => x.SaveCategory(It.IsAny<SeatCategory>()))
                .Returns(Task.FromResult(category));

            return this;
        }

        public MockCinemaDao MockGetCategory(SeatCategory category)
        {
            Setup(x => x.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(category));
            return this;
        }

        public MockCinemaDao MockGetCategoryById(SeatCategory category)
        {
            Setup(x => x.GetCategory(It.IsAny<int>()))
                .Returns(Task.FromResult(category));
            return this;
        }

        public MockCinemaDao MockRemoveCategory()
        {
            Setup(x => x.RemoveCategory(It.IsAny<SeatCategory>()))
                .Returns(Task.FromResult(1));
            return this;
        }

        public MockCinemaDao MockGetCategoriesForCinema(IEnumerable<SeatCategory> categories)
        {
            Setup(x => x.GetCategoriesForCinema(It.IsAny<int>()))
                .Returns(Task.FromResult(categories));
            return this;
        }

        public MockCinemaDao MockGetCategoryForCinemaByName(SeatCategory categories)
        {
            Setup(x => x.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(categories));
            return this;
        }
    }
}
