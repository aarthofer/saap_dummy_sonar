
package swt6.spring.worklog.config;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.CommandLineRunner;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;
import swt6.spring.worklog.domain.Employee;
import swt6.spring.worklog.domain.LogbookEntry;
import swt6.spring.worklog.logic.WorkLogService;

import java.time.LocalDate;
import java.time.LocalDateTime;

@Component
@Profile("dev")
public class DatabaseInitializer implements CommandLineRunner {

    private Logger logger = LoggerFactory.getLogger(DatabaseInitializer.class);

    private WorkLogService workLog;

    @Autowired
    public void setWorkLog(WorkLogService workLog) {
        this.workLog = workLog;
    }

    @Override
    public void run(String... args) {
        logger.info("Execute DatabaseInitializer");
        Employee empl1 = new Employee("Sepp", "Forcher", LocalDate.of(1935, 12, 12));
        empl1.addLogbookEntry(new LogbookEntry("Jour Fixe", LocalDateTime.of(2018, 3, 1, 8, 15), LocalDateTime.of(2018, 3, 1, 10, 0)));
        empl1.addLogbookEntry(new LogbookEntry("Analyse", LocalDateTime.of(2018, 3, 1, 10, 0), LocalDateTime.of(2018, 3, 1, 13, 45)));
        empl1.addLogbookEntry(new LogbookEntry("Implementierung", LocalDateTime.of(2018, 3, 1, 10, 15), LocalDateTime.of(2018, 3, 1, 14, 30)));
        workLog.syncEmployee(empl1);

        Employee empl2 = new Employee("Alfred", "Kunz", LocalDate.of(1944, 8, 10));
        empl2.addLogbookEntry(new LogbookEntry("Jour Fixe", LocalDateTime.of(2018, 3, 1, 8, 15), LocalDateTime.of(2018, 3, 1, 10, 0)));
        empl2.addLogbookEntry(new LogbookEntry("Unit-Test schreiben", LocalDateTime.of(2018, 3, 1, 10, 15), LocalDateTime.of(2018, 3, 1, 14, 30)));
        empl2.addLogbookEntry(new LogbookEntry("Integrations-Tests wiederholen", LocalDateTime.of(2018, 3, 1, 14, 30), LocalDateTime.of(2018, 3, 1, 16, 0)));
        workLog.syncEmployee(empl2);

        Employee empl3 = new Employee("Sigfried", "Hinz", LocalDate.of(1954, 5, 3));
        empl3.addLogbookEntry(new LogbookEntry("Jour Fixe", LocalDateTime.of(2018, 3, 1, 8, 15), LocalDateTime.of(2018, 3, 1, 10, 0)));
        empl3.addLogbookEntry(new LogbookEntry("Benutzerdoku aktualisieren", LocalDateTime.of(2018, 3, 1, 8, 15), LocalDateTime.of(2018, 3, 1, 16, 30)));
        workLog.syncEmployee(empl3);
    }
}