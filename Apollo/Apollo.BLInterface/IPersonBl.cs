using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BLInterface
{
    public interface IPersonBl
    {
        Task<Person> GetPersonByIdAsync(int id);

        Task<Person> CreatePersonAsync(Person Person);

        Task<Person> UpdatePersonAsync(Person Person);

        Task<int> DeletePersonByIdAsync(int id);

        Task<IEnumerable<Person>> GetPersons();
    }
}
