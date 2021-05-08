package swt6.spring.basics.aop.logic;

import java.util.List;

public interface WorkLogService {
  public Employee findEmployeeById(Long id) throws EmployeeIdNotFoundException;
  public List<Employee> findAllEmployees();
}