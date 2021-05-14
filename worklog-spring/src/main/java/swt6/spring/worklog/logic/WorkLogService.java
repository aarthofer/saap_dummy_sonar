package swt6.spring.worklog.logic;

import java.util.List;
import swt6.spring.worklog.domain.*;

public interface WorkLogService {
  public Employee       syncEmployee(Employee employee);
  public Employee       findEmployeeById(Long id);
  public List<Employee> findAllEmployees();
}