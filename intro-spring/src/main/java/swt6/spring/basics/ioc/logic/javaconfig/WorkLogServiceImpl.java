package swt6.spring.basics.ioc.logic.javaconfig;

import org.springframework.beans.factory.annotation.Autowired;
import swt6.spring.basics.ioc.domain.Employee;
import swt6.spring.basics.ioc.logic.WorkLogService;
import swt6.spring.basics.ioc.util.Log;
import swt6.spring.basics.ioc.util.Logger;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


public class WorkLogServiceImpl implements WorkLogService {

    private Map<Long, Employee> employees = new HashMap<>();

    @Autowired
    @Log(Log.Type.STANDARD)
    private Logger logger = null;

    public WorkLogServiceImpl() {
        init();
    }

    public WorkLogServiceImpl(Logger logger) {
        init();
        this.logger = logger;
    }

    public void setLogger(Logger logger) {
        this.logger = logger;
    }

    private void init() {
        employees.put(1L, new Employee(1L, "Bill", "Gates"));
        employees.put(2L, new Employee(2L, "James", "Goslin"));
        employees.put(3L, new Employee(3L, "Bjarne", "Stroustrup"));
    }

    public Employee findEmployeeById(Long id) {
        Employee empl = employees.get(id);
        logger.log("findEmployeeById(" + id + ") --> " +
                ((empl != null) ? empl.toString() : "<null>"));
        return empl;
    }

    public List<Employee> findAllEmployees() {
        logger.log("findAllEmployees()");
        return new ArrayList<Employee>(employees.values());
    }
}
