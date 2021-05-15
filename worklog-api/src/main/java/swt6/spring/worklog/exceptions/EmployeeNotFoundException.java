package swt6.spring.worklog.exceptions;

import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.ResponseStatus;

//TODO
public class EmployeeNotFoundException extends RuntimeException {

  private static final long serialVersionUID = 1L;

  public EmployeeNotFoundException(Long id) {
    super(String.format("Could not find employee '%d'.", id));
  }
}
