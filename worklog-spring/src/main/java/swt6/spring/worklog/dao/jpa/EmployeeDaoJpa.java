package swt6.spring.worklog.dao.jpa;

import org.springframework.dao.DataAccessException;
import swt6.spring.worklog.dao.EmployeeDao;
import swt6.spring.worklog.dao.jdbc.EmployeeDaoJdbc;
import swt6.spring.worklog.domain.Employee;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import java.util.List;

public class EmployeeDaoJpa implements EmployeeDao {

    @PersistenceContext
    private EntityManager entityManager;


    @Override
    public Employee findById(Long id) throws DataAccessException {
        return entityManager.find(Employee.class, id);
    }

    @Override
    public List<Employee> findAll() throws DataAccessException {
        return entityManager.createQuery( "select e from Employee e", Employee.class )
                .getResultList();
    }

    @Override
    public void insert(Employee employee) throws DataAccessException {
        entityManager.persist( employee );
        //employee.setId( persistedEmployee.getId() );
    }

    @Override
    public Employee merge(Employee employee) throws DataAccessException {
        return entityManager.merge( employee );
    }
}
