package swt6.spring.worklog.api;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import org.modelmapper.ModelMapper;
import org.modelmapper.TypeToken;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import swt6.spring.worklog.dto.EmployeeDto;
import swt6.spring.worklog.exceptions.EmployeeNotFoundException;
import swt6.spring.worklog.logic.WorkLogService;

import java.lang.reflect.Type;
import java.util.Collection;
import java.util.List;

@RestController
@RequestMapping(value = "/worklog", produces = MediaType.APPLICATION_JSON_VALUE)
public class EmployeeRestController {
    private Logger logger = LoggerFactory.getLogger(EmployeeRestController.class);

    private WorkLogService workLogService;

    @Autowired
    private ModelMapper modelMapper;

    @Autowired
    public void setWorkLogService(WorkLogService workLogService) {
        this.workLogService = workLogService;
    }

    public EmployeeRestController() {
        logger.info("EmployeeRestController constructed");
    }

    @GetMapping(value = "/employees", produces = MediaType.APPLICATION_JSON_VALUE)
    @Operation(summary = "Employee List", description = "Returns a list of all stored employees.")
    @ApiResponse(responseCode = "200", description = "Success")
    public List<EmployeeDto> getEmployees() {
        var employees = workLogService.findAllEmployees();
        Type listDtoType = new TypeToken<Collection<EmployeeDto>>() {
        }.getType();
        return modelMapper.map(employees, listDtoType);
    }

    @GetMapping(value = "/employees/{id}")
    @Operation(summary = "Employee Data", description = "Returns detailed data for a given employee")
    @ApiResponse(responseCode = "200", description = "Success")
    @ApiResponse(responseCode = "404", description = "Employee not found")
    public EmployeeDto getEmployeeById(@PathVariable long id) {
        var empl = this.workLogService.findEmployeeById(id);
        if (empl == null) {
            throw new EmployeeNotFoundException(id);
        }
        return modelMapper.map(empl, EmployeeDto.class);
    }

    @GetMapping(value = "hello", produces = MediaType.APPLICATION_JSON_VALUE)
    public String hello() {
        logger.info("EmployeeRestController.hello");
        return "Hello from EmployeeRestController";
    }
}
