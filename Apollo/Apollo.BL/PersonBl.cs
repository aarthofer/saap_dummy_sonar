using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class PersonBl : IPersonBl
    {
        private readonly IPersonDao personDao;

        public PersonBl(IPersonDao personDao)
        {
            personDao = personDao ?? throw new ArgumentNullException(nameof(personDao));
            this.personDao = personDao;
        }

        public async Task<Person> GetPersonByIdAsync(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }
            return await personDao.FindByIdAsync(id);
        }

        public async Task<Person> CreatePersonAsync(Person person)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }
            return await personDao.GetOrAddPersonByNameAsync(person.Name);
        }

        public async Task<Person> UpdatePersonAsync(Person person)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            if (person.Id < 0)
            {
                throw new ArgumentOutOfRangeException("ID must be >= 0");
            }

            return await personDao.UpdateAsync(person);
        }

        public async Task<int> DeletePersonByIdAsync(int id)
        {
            return await personDao.DeleteByIdAsync(id);
        }

        public async Task<IEnumerable<Person>> GetPersons()
        {
            return await personDao.FindAllAsync();
        }
    }
}
