package swt6.spring.worklog.api;

import org.modelmapper.ModelMapper;
import org.modelmapper.TypeToken;
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
import java.util.List;

@RestController
@RequestMapping( value = "worklog", produces = MediaType.APPLICATION_JSON_VALUE)
public class LogbookEntryRestController {

    @Autowired
    private WorkLogService workLogService;

    @Autowired
    private ModelMapper modelMapper;

    @GetMapping("/entries")
    public Collection<LogbookEntryDto> getLogbookEntriesForEmployee(@RequestParam Long employeeId) {
        var employee = workLogService.findEmployeeById( employeeId );
        if( employee == null ) {
            throw new EmployeeNotFoundException(employeeId);
        }
        List<LogbookEntry> entries = new ArrayList<>( employee.getLogbookEntries() );
        Comparator.comparing( LogbookEntry::getStartTime );
        Type listDtoType = new TypeToken<Collection<LogbookEntryDto>>(){}.getType();
        return modelMapper.map( entries, listDtoType );
    }


}
