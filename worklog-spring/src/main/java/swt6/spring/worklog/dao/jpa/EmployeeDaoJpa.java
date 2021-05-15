package swt6.spring.worklog.dao.jpa;

import swt6.spring.worklog.dao.EmployeeDao;
import swt6.spring.worklog.domain.Employee;

import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import java.util.List;

public class EmployeeDaoJpa implements EmployeeDao {

    @PersistenceContext
    private EntityManager entityManager;

    @Override
    public Employee findById(Long aLong) {
        return entityManager.find(Employee.class, aLong);
    }

    @Override
    public List<Employee> findAll() {
        final String sql = "SELECT e FROM Employee e";
        return entityManager.createQuery(sql, Employee.class).getResultList();
    }

    @Override
    public void insert(Employee employee) {
        entityManager.persist(employee);
//        employee.setId(e.getId());
    }

    @Override
    public Employee merge(Employee employee) {
        return entityManager.merge(employee);
    }
}
