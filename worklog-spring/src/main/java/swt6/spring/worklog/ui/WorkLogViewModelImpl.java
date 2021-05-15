package swt6.spring.worklog.ui;

import org.springframework.stereotype.Component;
import swt6.spring.worklog.domain.Employee;
import swt6.spring.worklog.logic.WorkLogService;

import java.util.Arrays;

public class WorkLogViewModelImpl implements WorkLogViewModel {

    private WorkLogService workLogService;

    public void setWorkLogService(WorkLogService workLogService) {
        this.workLogService = workLogService;
    }

    @Override
    public void saveEmployees(Employee... empls) {
        for (Employee e : empls) {
            Employee emp = workLogService.syncEmployee(e);
            e.setId(emp.getId());
        }
    }

    @Override
    public void findAll() {
        workLogService.findAllEmployees().forEach(e -> {
            System.out.println(e);
            e.getLogbookEntries().forEach(entry -> System.out.println("  " + entry));
        });
    }
}
