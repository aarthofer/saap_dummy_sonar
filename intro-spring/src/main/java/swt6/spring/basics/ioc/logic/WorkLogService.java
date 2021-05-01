package swt6.spring.basics.ioc.logic;

import java.util.List;
import swt6.spring.basics.ioc.domain.Employee;

public interface WorkLogService {
  public Employee       findEmployeeById(Long id);
  public List<Employee> findAllEmployees();
}
