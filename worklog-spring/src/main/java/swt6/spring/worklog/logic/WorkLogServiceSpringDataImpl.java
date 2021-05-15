package swt6.spring.worklog.logic;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Primary;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;
import swt6.spring.worklog.dao.EmployeeRepository;
import swt6.spring.worklog.domain.Employee;

import java.util.List;

@Component("workLog")
@Primary
@Transactional
public class WorkLogServiceSpringDataImpl implements WorkLogService{

    @Autowired
    private EmployeeRepository employeeRepository;

    @Override
    public Employee syncEmployee(Employee employee) {
        return employeeRepository.saveAndFlush( employee );
    }

    @Override
    @Transactional(readOnly = true)
    public Employee findEmployeeById(Long id) {
        return employeeRepository.findById( id ).orElse( null );
    }

    @Override
    @Transactional(readOnly = true)
    public List<Employee> findAllEmployees() {
        return employeeRepository.findAll();
    }

}
