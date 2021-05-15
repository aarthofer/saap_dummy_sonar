package swt6.spring.worklog.api;

import org.modelmapper.ModelMapper;
import org.modelmapper.TypeToken;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import swt6.spring.worklog.domain.LogbookEntry;
import swt6.spring.worklog.dto.LogbookEntryDto;
import swt6.spring.worklog.exceptions.EmployeeNotFoundException;
import swt6.spring.worklog.logic.WorkLogService;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Comparator;

@RestController
@RequestMapping(value = "worklog", produces = MediaType.APPLICATION_JSON_VALUE)
public class LogbookEntryRestController {
    private Logger logger = LoggerFactory.getLogger(LogbookEntryRestController.class);

    private WorkLogService workLogService;

    @Autowired
    private ModelMapper modelMapper;

    @Autowired
    public void setWorkLogService(WorkLogService workLogService) {
        this.workLogService = workLogService;
    }

    @GetMapping("/entries")
    public Collection<LogbookEntryDto> getLogbookEntriesForEmployee(@RequestParam long employeeId) {
        var employee = workLogService.findEmployeeById(employeeId);
        if (employee == null) {
            throw new EmployeeNotFoundException(employeeId);
        }

        var entries = new ArrayList<>(employee.getLogbookEntries());
        Comparator.comparing(LogbookEntry::getStartTime);

        Type listDtoType = new TypeToken<Collection<LogbookEntryDto>>() {
        }.getType();
        return modelMapper.map(entries, listDtoType);

    }
}
