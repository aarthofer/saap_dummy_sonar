using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Dal.Mock;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Linq;

namespace Apollo.BL.Test
{
    public class CinemaTest
    {

        private MockCinemaDao cinemaDaoMock = new MockCinemaDao();
        private MockCinemaHallDao cinemaHallDaoMock = new MockCinemaHallDao();

        [Fact]
        public async Task TestGetCinemaById()
        {
            Cinema cinema = new Cinema { CinemaHalls = new List<CinemaHall>(), Id = 1, Name = "Hagenberg Cineplexx" };
            cinemaDaoMock
                .MockGetById(cinema);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            Cinema cinemaDB = await cinemaBl.GetCinemaByIdAsync(1);
            Assert.Equal(cinemaDB.Name, cinema.Name);
            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCinemaByIdCinemaNotFound()
        {
            cinemaDaoMock
                .MockGetByInvalidId();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            Cinema cinemaDB = await cinemaBl.GetCinemaByIdAsync(999);
            Assert.Null(cinemaDB);

            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCinemaByIdInvalid()
        {
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.GetCinemaByIdAsync(-1));

            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TestUpdateCinema()
        {
            Cinema newCinema = new Cinema { Id = 1, Name = "Cineplexx", CinemaHalls = new List<CinemaHall>() };
            Cinema updatedCinema = new Cinema { Id = 1, Name = "Cineplexx Updated", CinemaHalls = new List<CinemaHall>() };

            cinemaDaoMock
                .MockCreateCinema(newCinema)
                .MockUpdateCinema(updatedCinema);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            Cinema insertedCinema = await cinemaBl.CreateCinemaAsync(newCinema);
            Cinema cinema = await cinemaBl.UpdateCinemaAsync(updatedCinema);

            Assert.NotEqual(insertedCinema.Name, cinema.Name);
            cinemaDaoMock.Verify(dao => dao.CreateAsync(It.IsAny<Cinema>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.UpdateAsync(It.IsAny<Cinema>()), Times.Once);
        }

        [Fact]
        public async Task TestUpdateCinemaWrongId()
        {
            Cinema updatedCinema = new Cinema { Id = -1, Name = "Cineplexx Updated", CinemaHalls = new List<CinemaHall>() };

            cinemaDaoMock
                .MockGetById(null)
                .MockUpdateCinema(updatedCinema);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.UpdateCinemaAsync(updatedCinema));
        }

        [Fact]
        public async Task TestUpdateCinemaNull()
        {
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => cinemaBl.UpdateCinemaAsync(null));
        }

        [Fact]
        public async Task TestCreateCinema()
        {
            Cinema insertCinema = new Cinema { CinemaHalls = new List<CinemaHall>(), Name = "ViennaTestCineplexx", Id= 1 };
            
            cinemaDaoMock
                .MockFindCinema(null)
                .MockCreateCinema(insertCinema);
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            Cinema newCinema = await cinemaBl.CreateCinemaAsync(insertCinema);

            Assert.NotNull(newCinema);
            Assert.NotEqual(-1, newCinema.Id);
            Assert.Equal("ViennaTestCineplexx", newCinema.Name);

            cinemaDaoMock.Verify(dao => dao.FindCinemaByName(It.IsAny<string>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.CreateAsync(It.IsAny<Cinema>()), Times.Once);
        }

        [Fact]
        public async Task TestCreateSameCinemaAgain()
        {
            Cinema insertCinema = new Cinema { CinemaHalls = new List<CinemaHall>(), Name = "ViennaTestCineplexx", Id = 1 };

            cinemaDaoMock
                .MockFindCinema(insertCinema)
                .MockCreateCinema(insertCinema);
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.CreateCinemaAsync(insertCinema));
            cinemaDaoMock.Verify(dao => dao.FindCinemaByName(It.IsAny<string>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.CreateAsync(It.IsAny<Cinema>()), Times.Never);
        }

        [Fact]
        public async Task TestCreateInvalidCinema()
        {
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => cinemaBl.CreateCinemaAsync(null));
            cinemaDaoMock.Verify(dao => dao.FindCinemaByName(It.IsAny<string>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.CreateAsync(It.IsAny<Cinema>()), Times.Never);
        }

        [Fact]
        public async Task TestRemoveCinema()
        {
            cinemaDaoMock.MockRemoveCinema();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            int removed = await cinemaBl.DeleteCinemaAsync(new Cinema{ Id=1, Name="" });
            Assert.Equal(1, removed);
            cinemaDaoMock.Verify(x => x.RemoveCinema(It.IsAny<Cinema>()), Times.Once);
        }

        [Fact]
        public async Task TestRemoveCinemaById()
        {
            cinemaDaoMock
                .MockGetById(new Cinema { Id= 1, Name="Test"})
                .MockRemoveCinema();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            int removed = await cinemaBl.DeleteCinemaAsync(1);
            Assert.Equal(1, removed);
            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.RemoveCinema(It.IsAny<Cinema>()), Times.Once);
        }

        [Fact]
        public async Task TestRemoveCinemaByIdInvalidId()
        {
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.DeleteCinemaAsync(-1));

            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.RemoveCinema(It.IsAny<Cinema>()), Times.Never);
        }

        [Fact]
        public async Task TestRemoveCinemaByIdCinemaDoesNotExist()
        {
            cinemaDaoMock.MockGetByInvalidId();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.DeleteCinemaAsync(999));

            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.RemoveCinema(It.IsAny<Cinema>()), Times.Never);
        }

        [Fact]
        public async Task TestRemoveCinemaNull()
        {
            cinemaDaoMock.MockRemoveCinema();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => cinemaBl.DeleteCinemaAsync(null));
            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.RemoveCinema(It.IsAny<Cinema>()), Times.Never);
        }

        [Fact]
        public async Task TestRemoveCinemaInvalidId()
        {
            cinemaDaoMock.MockRemoveCinema();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.DeleteCinemaAsync(new Cinema()));
            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.RemoveCinema(It.IsAny<Cinema>()), Times.Never);
        }

        [Fact]
        public async Task TestCreateNewCategory()
        {
            SeatCategory category = new SeatCategory { Id = 1, Name = "Most Expensive Category", Price = 999.99, CinemaId = 1 };

            cinemaDaoMock
                .MockGetCategory(null)
                .MockSaveCategory(category);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            SeatCategory newCategory = await cinemaBl.SaveCategoryAsync(category);

            Assert.Equal("Most Expensive Category", newCategory.Name);
            Assert.Equal(999.99, newCategory.Price);
            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.SaveCategory(It.IsAny<SeatCategory>()), Times.Once);
        }

        [Fact]
        public async Task TestUpdateCategory()
        {
            SeatCategory newCategory = new SeatCategory { Id = 1, Name = "Most Expensive Category", Price = 999.99, CinemaId = 1 };
            SeatCategory updatedCategory = new SeatCategory { Id = 1, Name = "Most Expensive Category", Price = 1.99, CinemaId = 1};

            cinemaDaoMock
                .MockGetCategory(newCategory)
                .MockSaveCategory(updatedCategory);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            SeatCategory category = await cinemaBl.SaveCategoryAsync(updatedCategory);

            Assert.Equal("Most Expensive Category", category.Name);
            Assert.Equal(1.99, category.Price);
            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.SaveCategory(It.IsAny<SeatCategory>()), Times.Once);
        }

        [Fact]
        public async Task TestDontUpdateCategory()
        {
            SeatCategory newCategory = new SeatCategory { Id = 1, Name = "Most Expensive Category", Price = 999.99 };
            SeatCategory updatedCategory = new SeatCategory { Id = 1, Name = "Most Expensive Category", Price = 999.99 };

            cinemaDaoMock
                .MockGetCategory(newCategory)
                .MockSaveCategory(updatedCategory);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            SeatCategory category = await cinemaBl.SaveCategoryAsync(updatedCategory);

            Assert.Equal("Most Expensive Category", category.Name);
            Assert.Equal(999.99, category.Price);
            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            cinemaDaoMock.Verify(dao => dao.SaveCategory(It.IsAny<SeatCategory>()), Times.Never);
        }

        [Fact]
        public async Task TestInvalidSaveCategory()
        {
            SeatCategory newCategory = new SeatCategory { Id = 1, Name = "Most Expensive Category", Price = 999.99 };
            SeatCategory updatedCategory = new SeatCategory { Id = 1, Name = "Most Expensive Category", Price = 999.99 };

            cinemaDaoMock
                .MockGetCategory(newCategory)
                .MockSaveCategory(updatedCategory);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => cinemaBl.SaveCategoryAsync(null));

            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.SaveCategory(It.IsAny<SeatCategory>()), Times.Never);
        }

        [Fact]
        public async Task TestGetCategoriesForCinemaById()
        {
            List<SeatCategory> categories = new List<SeatCategory>();
            categories.Add(new SeatCategory { Id = 1, Name = "Luxury", Price = 100, CinemaId = 1 });
            categories.Add(new SeatCategory { Id = 2, Name = "Second class", Price = 10, CinemaId = 1 });
            categories.Add(new SeatCategory { Id = 1, Name = "Third Class", Price = 1, CinemaId = 1 });

            cinemaDaoMock
                .MockGetCategoriesForCinema(categories);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            IEnumerable<SeatCategory> cinemaCategories = await cinemaBl.GetCategoriesForCinemaAsync(1);
            Assert.Equal(3, cinemaCategories.Count());
            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.GetCategoriesForCinema(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCategoriesForCinemaByIdInvalidId()
        {
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.GetCategoriesForCinemaAsync(-1));

            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.GetCategoriesForCinema(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TestGetCategoriesForCinema()
        {
            List<SeatCategory> categories = new List<SeatCategory>();
            categories.Add(new SeatCategory { Id = 1, Name = "Luxury", Price = 100, CinemaId = 1 });
            categories.Add(new SeatCategory { Id = 2, Name = "Second class", Price = 10, CinemaId = 1 });
            categories.Add(new SeatCategory { Id = 1, Name = "Third Class", Price = 1, CinemaId = 1 });

            cinemaDaoMock
                .MockGetById(new Cinema { Id = 1, Name = "Cinema", CinemaHalls = new List<CinemaHall>() })
                .MockGetCategoriesForCinema(categories);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            IEnumerable<SeatCategory> cinemaCategories = await cinemaBl.GetCategoriesForCinemaAsync(1);
            Assert.Equal(3, cinemaCategories.Count());
            cinemaDaoMock.Verify(dao => dao.FindByIdAsync(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.GetCategoriesForCinema(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCategoryById()
        {
            SeatCategory cat = new SeatCategory { Id = 1, Name = "Category", Price = 100, CinemaId = 1 };

            cinemaDaoMock
                .MockGetCategoryById(cat);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            SeatCategory newCat = await cinemaBl.GetCategoryByIdAsync(1);

            cinemaDaoMock.Verify(dao => dao.GetCategoriesForCinema(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.GetCategory(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCategoryByNonExistantId()
        {
            cinemaDaoMock
                .MockGetCategoryById(null);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            SeatCategory newCat = await cinemaBl.GetCategoryByIdAsync(1000);

            Assert.Null(newCat);
            cinemaDaoMock.Verify(dao => dao.GetCategoriesForCinema(It.IsAny<int>()), Times.Never);
            cinemaDaoMock.Verify(dao => dao.GetCategory(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCategoryByInvalidId()
        {
            cinemaDaoMock
                .MockGetCategoryById(null);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.GetCategoryByIdAsync(-1));
        }

        [Fact]
        public async Task TestGetCategoryBaIdAndString()
        {
            cinemaDaoMock
                .MockGetCategoryForCinemaByName(new SeatCategory { Id = 1, CinemaId = 1, Name = "Cat", Price = 0 });

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            SeatCategory category = await cinemaBl.GetCategoryForCinemaByNameAsync(1, "Cat");
            Assert.Equal("Cat", category.Name);
            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCategoryBaIdAndNullString()
        {
            cinemaDaoMock
                .MockGetCategoryForCinemaByName(new SeatCategory { Id = 1, CinemaId = 1, Name = "Cat", Price = 0 });

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => cinemaBl.GetCategoryForCinemaByNameAsync(1, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => cinemaBl.GetCategoryForCinemaByNameAsync(1, ""));

            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TestGetCategoryBaIdAndStringNotExistent()
        {
            cinemaDaoMock
                .MockGetCategoryForCinemaByName(null);

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            SeatCategory cat = await cinemaBl.GetCategoryForCinemaByNameAsync(1, "Not existing Category");

            Assert.Null(cat);
            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TestGetCategoryInvalidID()
        {
            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>( () => cinemaBl.GetCategoryForCinemaByNameAsync(-1, "Not existing Category"));

            cinemaDaoMock.Verify(dao => dao.GetCategoryForCinema(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task TestDeleteCategory()
        {
            cinemaDaoMock
                .MockRemoveCategory();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            int deleted = await cinemaBl.DeleteCategoryByIdAsync(1);
            cinemaDaoMock.Verify(dao => dao.RemoveCategory(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task TestDeleteCategoryByIdInvalidId()
        {
            cinemaDaoMock
                .MockRemoveCategory();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.DeleteCategoryByIdAsync(-1));
            cinemaDaoMock.Verify(dao => dao.RemoveCategory(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TestDeleteCategoryByObject()
        {
            cinemaDaoMock
                .MockRemoveCategory();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            int deleted = await cinemaBl.DeleteCategoryAsync(new SeatCategory { Id=1, CinemaId=1, Name="Test", Price = 1 });
            Assert.Equal(1, deleted);
            cinemaDaoMock.Verify(dao => dao.RemoveCategory(It.IsAny<SeatCategory>()), Times.Once);
        }

        [Fact]
        public async Task TestDeleteCategoryNull()
        {
            cinemaDaoMock
                .MockRemoveCategory();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => cinemaBl.DeleteCategoryAsync(null));
            cinemaDaoMock.Verify(dao => dao.RemoveCategory(It.IsAny<SeatCategory>()), Times.Never);
        }

        [Fact]
        public async Task TestDeleteCategoryInvalidId()
        {
            cinemaDaoMock
                .MockRemoveCategory();

            ICinemaBl cinemaBl = new CinemaBl(cinemaDaoMock.Object, cinemaHallDaoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => cinemaBl.DeleteCategoryAsync(new SeatCategory { CinemaId = 1, Id = -1 }));
            cinemaDaoMock.Verify(dao => dao.RemoveCategory(It.IsAny<SeatCategory>()), Times.Never);
        }
    }
}
