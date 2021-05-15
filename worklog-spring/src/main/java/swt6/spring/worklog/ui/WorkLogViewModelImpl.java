package swt6.spring.worklog.ui;

import org.springframework.transaction.annotation.Transactional;
import swt6.spring.worklog.domain.Employee;
import swt6.spring.worklog.logic.WorkLogService;

public class WorkLogViewModelImpl implements WorkLogViewModel {

    private WorkLogService workLogService;

    public void setWorkLogService(WorkLogService workLogService  ) {
        this.workLogService = workLogService;
    }

    @Override
    public void saveEmployees(Employee... empls) {
        for( Employee e : empls ) {
            Employee tmp = workLogService.syncEmployee( e );
            e.setId( tmp.getId() );
        }
    }

    @Override
    public void findAll() {
        for( Employee e : workLogService.findAllEmployees() ) {
            System.out.println( e );
            e.getLogbookEntries().forEach( entry -> System.out.println( "  " + entry ) );
        }
    }
}
