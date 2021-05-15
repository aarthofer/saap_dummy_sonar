package swt6.spring.worklog.api;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import org.modelmapper.TypeToken;
import org.modelmapper.ModelMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import swt6.spring.worklog.domain.Employee;
import swt6.spring.worklog.dto.EmployeeDto;
import swt6.spring.worklog.exceptions.EmployeeNotFoundException;
import swt6.spring.worklog.logic.WorkLogService;

import java.lang.reflect.Type;
import java.util.Collection;
import java.util.List;

@RestController
@RequestMapping( value = "/worklog", produces = MediaType.APPLICATION_JSON_VALUE)
public class EmployeeRestController {

    private Logger logger = LoggerFactory.getLogger( EmployeeRestController.class );

    @Autowired
    private WorkLogService workLogService;
    @Autowired
    private ModelMapper modelMapper;

    public EmployeeRestController() {
        logger.info( "EmployeeRestController constructed" );
    }

    @GetMapping( value = "hello", produces = MediaType.TEXT_PLAIN_VALUE)
    public String hello() {
        logger.info( "EmployeeRestController.hello()" );
        return "Hello from EmployeeRestController";
    }

    @GetMapping("/employees")
    @Operation( summary = "Employee List", description = "Returns a list of all stored employees.")
    @ApiResponse( responseCode = "200", description = "Success")
    public List<EmployeeDto> getEmployees() {
        var employees = this.workLogService.findAllEmployees();
        Type listDtoType = new TypeToken<Collection<EmployeeDto>>(){}.getType();
        return modelMapper.map( employees, listDtoType );
    }

    @GetMapping( "/employees/{id}" )
    @Operation( summary = "Employee Data", description = "Returns details data for a given employee.")
    @ApiResponse( responseCode = "200", description = "Success")
    @ApiResponse( responseCode = "404", description = "Employee entry not found")
    public EmployeeDto getEmployeeById(@PathVariable Long id) {
        var employee = this.workLogService.findEmployeeById( id );
        if( employee == null ) {
            throw new EmployeeNotFoundException( id );
        }
        return modelMapper.map( employee, EmployeeDto.class );
    }

}
