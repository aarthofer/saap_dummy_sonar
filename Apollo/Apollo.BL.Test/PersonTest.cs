using Apollo.BLInterface;
using Apollo.Core.Dal.Mock;
using Apollo.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Apollo.BL.Test
{
    public class PersonTest
    {
        IPersonBl personBl;
        MockPersonDao personService = new MockPersonDao();

        public PersonTest()
        {
            personService.Init();

            personBl = new PersonBl(personService.Object);
        }

        [Fact]
        public async Task TestGetPersons()
        {
            IEnumerable<Person> persons = await personBl.GetPersons();

            Assert.NotEmpty(persons);
        }

        [Fact]
        public async Task TestGetPersonById()
        {
            Person r = await personBl.GetPersonByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Person 1", r.Name);
        }

        [Fact]
        public async Task TestGetPersonByInvalidId()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => personBl.GetPersonByIdAsync(-1));
        }

        [Fact]
        public async Task TestDeletePersonId()
        {
            Person r = await personBl.GetPersonByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Person 1", r.Name);
            Assert.Equal(1, await personBl.DeletePersonByIdAsync(1));
            Assert.Equal(0, await personBl.DeletePersonByIdAsync(1));
        }

        [Fact]
        public async Task TestDeletePersonByInvalidId()
        {
            Assert.Equal(0, await personBl.DeletePersonByIdAsync(-1));
        }

        [Fact]
        public async Task TestAddPerson()
        {
            Person r = await personBl.CreatePersonAsync(new Person() { Name = "TestPerson" });

            Assert.NotNull(r);
            Assert.Equal("TestPerson", r.Name);

            personService.Verify(r => r.GetOrAddPersonByNameAsync(It.IsAny<string>()), Times.Once());

            personService.Verify(r => r.CreateAsync(It.IsAny<Person>()), Times.Never());
        }

        [Fact]
        public async Task TestAddPersonNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => personBl.CreatePersonAsync(null));
        }

        [Fact]
        public async Task TestUpdatePerson()
        {
            Person r = await personBl.GetPersonByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Person 1", r.Name);
            r.Name = "updated Person";
            Person r1 = await personBl.UpdatePersonAsync(r);
            Assert.NotNull(r1);
            Assert.Equal("updated Person", r1.Name);
        }

        [Fact]
        public async Task TestUpdatePersonInvalidId()
        {
            Person r = await personBl.GetPersonByIdAsync(1);
            Assert.NotNull(r);
            Assert.Equal("Person 1", r.Name);
            r.Id = -1;
            r.Name = "updated Person";
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => personBl.UpdatePersonAsync(r));
            personService.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Never());
        }

        [Fact]
        public async Task TestUpdatePersonNotExistingId()
        {
            Person r = new Person();
            r.Id = 99;
            r.Name = "updated Person";
            Person r1 = await personBl.UpdatePersonAsync(r);
            Assert.Null(r1);
            personService.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Once());
        }

        [Fact]
        public async Task TestUpdatePersonNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => personBl.UpdatePersonAsync(null));
            personService.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Never());
        }
    }
}
