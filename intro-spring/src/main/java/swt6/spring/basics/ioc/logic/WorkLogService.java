package swt6.spring.basics.ioc.logic;

import swt6.spring.basics.ioc.domain.Employee;

import java.util.List;

public interface WorkLogService {
    public Employee findEmployeeById(Long id);

    public List<Employee> findAllEmployees();
}
