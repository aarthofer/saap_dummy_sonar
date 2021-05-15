package swt6.spring.worklog.logic;

import swt6.spring.worklog.domain.Employee;

import java.util.List;

public interface WorkLogService {
    public Employee syncEmployee(Employee employee);

    public Employee findEmployeeById(Long id);

    public List<Employee> findAllEmployees();
}