using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Dal.Mock;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.BL.Test
{
    public class MovieTest
    {
        MockGenreDao genreService = new MockGenreDao();
        MockRoleDao roleService = new MockRoleDao();
        MockMovieDao movieService = new MockMovieDao();

        IMovieBl movieBl;

        public MovieTest()
        {
            roleService
                .MockCreateAsync()
                .MockFindAllAsync()
                .MockFindByIdAsync()
                .MockUpdateAsync()
                .MockGetOrAddRoleByNameAsync()
                .MockDeleteByIdAsync();

            movieService.Init();

            genreService.Init();

            movieBl = new MovieBl(movieService.Object, genreService.Object, roleService.Object);
        }

        [Fact]
        public async Task TestCreateMovieBl()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => new MovieBl(null, null, null));

            var genEx = await Assert.ThrowsAsync<ArgumentNullException>(async () => new MovieBl(movieService.Object, null, roleService.Object));
            Assert.Equal($"Value cannot be null. (Parameter 'genreDao')", genEx.Message);

            var movieEx = await Assert.ThrowsAsync<ArgumentNullException>(async () => new MovieBl(null, genreService.Object, roleService.Object));
            Assert.Equal($"Value cannot be null. (Parameter 'movieDao')", movieEx.Message);

            var roleEx = await Assert.ThrowsAsync<ArgumentNullException>(async () => new MovieBl(movieService.Object, genreService.Object, null));
            Assert.Equal($"Value cannot be null. (Parameter 'roleDao')", roleEx.Message);
        }

        [Fact]
        public async Task TestGetMovie()
        {
            Movie m = await movieBl.GetMovieByIdAsync(1);
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Once());
            Assert.NotNull(m);
            Assert.Equal(1, m.Id);
        }

        [Fact]
        public async Task TestGetMovieInvalidId()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await movieBl.GetMovieByIdAsync(-1));
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Never());
        }

        [Fact]
        public async Task TestDeleteMovie()
        {
            Movie m = await movieBl.GetMovieByIdAsync(1);
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Once());
            Assert.NotNull(m);
            Assert.Equal(1, m.Id);

            int i = await movieBl.DeleteMovieByIdAsync(1);
            Assert.Equal(1, i);

            int i1 = await movieBl.DeleteMovieByIdAsync(1);
            Assert.Equal(0, i1);

            Movie mD = await movieBl.GetMovieByIdAsync(1);
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.AtMost(2));
            Assert.Null(mD);
        }

        [Fact]
        public async Task TestDeleteMovieInvalidId()
        {
            Movie mD = await movieBl.GetMovieByIdAsync(77);
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Once());
            Assert.Null(mD);

            int count = await movieBl.DeleteMovieByIdAsync(77);
            Assert.Equal(0, count);
            movieService.Verify(g => g.DeleteByIdAsync(It.IsAny<int>()), Times.Once());
        }

        [Fact]
        public async Task TestCreateMovie()
        {
            Movie nM = GetNewMovie();
            nM.Id = -1;
            Movie m = await movieBl.CreateMovieAsync(nM);
            movieService.Verify(g => g.CreateAsync(It.IsAny<Movie>()), Times.Once());
            Assert.NotNull(m);
            Assert.NotEqual(-1, m.Id);

            Movie mA = await movieBl.GetMovieByIdAsync(m.Id);
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Once());
            Assert.NotNull(mA);
        }

        [Fact]
        public async Task TestCreateMovieInvalid()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await movieBl.CreateMovieAsync(null));
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Never());
        }


        [Fact]
        public async Task TestUpdateMovie()
        {
            Movie mA = await movieBl.GetMovieByIdAsync(1);
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Once());
            Assert.NotNull(mA);

            mA.Title = "New Title";
            Movie m = await movieBl.UpdateMovieAsync(mA);
            movieService.Verify(g => g.UpdateAsync(It.IsAny<Movie>()), Times.Once());
            Assert.NotNull(m);
            Assert.Equal(1, m.Id);
            Assert.Equal("New Title", m.Title);
        }

        [Fact]
        public async Task TestUpdateMovieInvalidId()
        {
            Movie mA = await movieBl.GetMovieByIdAsync(1);
            movieService.Verify(g => g.FindByIdAsync(It.IsAny<int>()), Times.Once());
            Assert.NotNull(mA);

            mA.Id = -1;
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await movieBl.UpdateMovieAsync(mA));
            movieService.Verify(g => g.UpdateAsync(It.IsAny<Movie>()), Times.Never());
        }

        [Fact]
        public async Task TestUpdateMovieInvalidNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await movieBl.UpdateMovieAsync(null));
            movieService.Verify(g => g.UpdateAsync(It.IsAny<Movie>()), Times.Never());
        }

        private Movie GetNewMovie()
        {
            return new Movie()
            {
                Id = 9,
                Title = "Movie 9",
                Description = "Description 9",
                DurationMinutes = 100,
                Genre = new List<Genre>() { new Genre() { Id = 1, Name = "Genre 1" } },
                Image = "Image",
                ReleaseDate = new DateTime(2020, 10, 10),
                Actors = new List<Person>() { new Person() { Id = 1, Name = "Person 1" } },
                Crew = new List<KeyValuePair<Person, string>>() { new KeyValuePair<Person, string>(new Person() { Id = 2, Name = "Person 2" }, "Director") }
            };
        }

        [Fact]
        public async Task TestGetRoleById()
        {
            Role r = await movieBl.GetRoleByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Role 1", r.Name);
        }

        [Fact]
        public async Task TestGetRoleByInvalidId()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => movieBl.GetRoleByIdAsync(-1));
        }

        [Fact]
        public async Task TestDeleteRoleId()
        {
            Role r = await movieBl.GetRoleByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Role 1", r.Name);
            Assert.Equal(1, await movieBl.RemoveRoleAsync(1));
            Assert.Equal(0, await movieBl.RemoveRoleAsync(1));
        }

        [Fact]
        public async Task TestDeleteRoleByInvalidId()
        {
            Assert.Equal(0, await movieBl.RemoveRoleAsync(-1));
        }

        [Fact]
        public async Task TestGetAllRoles()
        {
            IEnumerable<Role> roles = await movieBl.GetRolesAsync();

            Assert.NotEmpty(roles);

            roleService.Verify(r => r.FindAllAsync(), Times.Once());
        }


        [Fact]
        public async Task TestAddRole()
        {
            Role r = await movieBl.AddRoleAsync(new Role() { Name = "New Role" });

            Assert.NotNull(r);
            Assert.Equal("New Role", r.Name);

            roleService.Verify(r => r.GetOrAddRoleByNameAsync(It.IsAny<string>()), Times.Once());

            roleService.Verify(r => r.CreateAsync(It.IsAny<Role>()), Times.Never());
        }

        [Fact]
        public async Task TestAddRoleNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => movieBl.AddRoleAsync(null));
        }

        [Fact]
        public async Task TestUpdateRole()
        {
            Role r = await movieBl.GetRoleByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Role 1", r.Name);
            r.Name = "updated role";
            Role r1 = await movieBl.UpdateRoleAsync(r);
            Assert.NotNull(r1);
            Assert.Equal("updated role", r1.Name);
        }

        [Fact]
        public async Task TestUpdateRoleInvalidId()
        {
            Role r = await movieBl.GetRoleByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Role 1", r.Name);
            r.Id = -1;
            r.Name = "updated role";
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => movieBl.UpdateRoleAsync(r));
            roleService.Verify(r => r.UpdateAsync(It.IsAny<Role>()), Times.Never());
        }

        [Fact]
        public async Task TestUpdateRoleNotExistingId()
        {
            Role r = new Role();
            r.Id = 99;
            r.Name = "updated role";
            Role r1 = await movieBl.UpdateRoleAsync(r);
            Assert.Null(r1);
            roleService.Verify(r => r.UpdateAsync(It.IsAny<Role>()), Times.Once());
        }

        [Fact]
        public async Task TestUpdateRoleNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => movieBl.UpdateRoleAsync(null));
            roleService.Verify(r => r.UpdateAsync(It.IsAny<Role>()), Times.Never());
        }

        [Fact]
        public async Task TestGetGenreById()
        {
            Genre r = await movieBl.GetGenreByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Genre 1", r.Name);
        }

        [Fact]
        public async Task TestGetGenreByInvalidId()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => movieBl.GetGenreByIdAsync(-1));
        }

        [Fact]
        public async Task TestDeleteGenreId()
        {
            Genre r = await movieBl.GetGenreByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Genre 1", r.Name);
            Assert.Equal(1, await movieBl.RemoveGenreAsync(1));
            Assert.Equal(0, await movieBl.RemoveGenreAsync(1));
        }

        [Fact]
        public async Task TestDeleteGenreByInvalidId()
        {
            Assert.Equal(0, await movieBl.RemoveGenreAsync(-1));
        }

        [Fact]
        public async Task TestGetAllGenres()
        {
            IEnumerable<Genre> genres = await movieBl.GetGenresAsync();

            Assert.NotEmpty(genres);

            genreService.Verify(r => r.FindAllAsync(), Times.Once());
        }

        [Fact]
        public async Task TestAddGenre()
        {
            Genre r = await movieBl.AddGenreAsync(new Genre() { Name = "New Genre" });

            Assert.NotNull(r);
            Assert.Equal("New Genre", r.Name);

            genreService.Verify(r => r.GetOrAddGenreByNameAsync(It.IsAny<string>()), Times.Once());

            genreService.Verify(r => r.CreateAsync(It.IsAny<Genre>()), Times.Never());
        }

        [Fact]
        public async Task TestAddGenreNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => movieBl.AddGenreAsync(null));
        }

        [Fact]
        public async Task TestUpdateGenre()
        {
            Genre r = await movieBl.GetGenreByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Genre 1", r.Name);
            r.Name = "updated Genre";
            Genre r1 = await movieBl.UpdateGenreAsync(r);
            Assert.NotNull(r1);
            Assert.Equal("updated Genre", r1.Name);
        }

        [Fact]
        public async Task TestUpdateGenreInvalidId()
        {
            Genre r = await movieBl.GetGenreByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Genre 1", r.Name);
            r.Id = -1;
            r.Name = "updated Genre";
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => movieBl.UpdateGenreAsync(r));
            genreService.Verify(r => r.UpdateAsync(It.IsAny<Genre>()), Times.Never());
        }

        [Fact]
        public async Task TestUpdateGenreNotExistingId()
        {
            Genre r = new Genre();
            r.Id = 99;
            r.Name = "updated Genre";
            Genre r1 = await movieBl.UpdateGenreAsync(r);
            Assert.Null(r1);
            genreService.Verify(r => r.UpdateAsync(It.IsAny<Genre>()), Times.Once());
        }

        [Fact]
        public async Task TestUpdateGenreNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => movieBl.UpdateGenreAsync(null));
            genreService.Verify(r => r.UpdateAsync(It.IsAny<Genre>()), Times.Never());
        }
    }
}
