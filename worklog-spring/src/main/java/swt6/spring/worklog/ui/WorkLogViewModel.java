package swt6.spring.worklog.ui;

import swt6.spring.worklog.domain.Employee;

// Interface that should be used by rich clients. Implementations
// of this interface can hold the state of the user interface.
// All event handlers should invoke methods of WorkLogViewModel
// if they access business logic.
public interface WorkLogViewModel {
    public void saveEmployees(Employee... empls);

    public void findAll();
}
