package swt6.spring.worklog.dao;

import swt6.spring.worklog.domain.Employee;

import java.util.List;

public interface EmployeeDao{

        Employee findById(Long id);
        List<Employee> findAll();
        void insert(Employee e);
        Employee merge(Employee e);
        // void delete(Employee e);
}

