package swt6.spring.basics.ioc.logic.javaconfig;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import swt6.spring.basics.ioc.domain.Employee;
import swt6.spring.basics.ioc.logic.WorkLogService;
import swt6.spring.basics.ioc.util.FileLogger;


public class WorkLogServiceImpl implements WorkLogService {

    private Map<Long, Employee> employees = new HashMap<>();

    private FileLogger logger = null;

    private void initLogger() {
        //TODO
        logger = new FileLogger("log.txt");
    }

    private void init() {
        employees.put(1L, new Employee(1L, "Bill", "Gates"));
        employees.put(2L, new Employee(2L, "James", "Goslin"));
        employees.put(3L, new Employee(3L, "Bjarne", "Stroustrup"));
    }

    public WorkLogServiceImpl() {
        init();
        initLogger();
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
