package swt6.spring.test.worklog;


import static swt6.util.PrintUtil.printTitle;
import java.time.LocalDate;
import java.time.LocalDateTime;
import javax.persistence.EntityManagerFactory;

import org.junit.Ignore;
import org.junit.Test;
import org.springframework.context.support.AbstractApplicationContext;
import org.springframework.context.support.ClassPathXmlApplicationContext;
import swt6.spring.worklog.domain.Employee;
import swt6.spring.worklog.domain.LogbookEntry;
import swt6.spring.worklog.logic.WorkLogService;


public class LogicTest {

  @Ignore
  @Test
  public void testBusinessLogicWithJpaDaos() {
    printTitle("testBusinessLogicWithJpaDaos", 60);

    try (AbstractApplicationContext factory =
        new ClassPathXmlApplicationContext(
                "swt6/spring/worklog/test/applicationContext-jpa.xml")) {
      
      EntityManagerFactory emFactory = factory.getBean(EntityManagerFactory.class);
      final WorkLogService workLog = factory.getBean("workLog", WorkLogService.class);

      //TODO fix me

      printTitle("saveEmployee", 60, '-');
      saveEmployee(workLog);
  
      printTitle("addLogbookEntry", 60, '-');
      addLogbookEntry(workLog);

      printTitle("findAll", 60, '-');
      findAll(workLog);

    }

  }

  @Ignore
  @Test
  public void testBusinessLogicWithSpringDataRepositories() {
    printTitle("testBusinessLogicWithSpringDataRepositories", 60);
    try (AbstractApplicationContext factory =
        new ClassPathXmlApplicationContext(
            "swt6/spring/worklog/test/applicationContext-data.xml")) {
      
      EntityManagerFactory emFactory = factory.getBean(EntityManagerFactory.class);
      final WorkLogService workLog = factory.getBean("workLog", WorkLogService.class);

      //TODO fix me

      printTitle("saveEmployee", 60, '-');
      saveEmployee(workLog);
  
      printTitle("addLogbookEntry", 60, '-');
      addLogbookEntry(workLog);
  
      printTitle("findAll", 60, '-');
      findAll(workLog);
    }

  }

  private Employee empl1;
  private Employee empl2;
  private Employee empl3;

  private void saveEmployee(WorkLogService workLog) {
    empl1 = new Employee("Sepp", "Forcher", LocalDate.of(1935, 12, 12));
    empl2 = new Employee("Alfred", "Kunz", LocalDate.of(1944, 8, 10));
    empl3 = new Employee("Sigfried", "Hinz", LocalDate.of(1954, 5, 3));

    empl1 = workLog.syncEmployee(empl1);
    empl2 = workLog.syncEmployee(empl2);
    empl3 = workLog.syncEmployee(empl3);
  }

  private void addLogbookEntry(WorkLogService workLog) {
    LogbookEntry entry1 = new LogbookEntry("Analyse",
            LocalDateTime.of(2018, 3, 1, 10, 0), LocalDateTime.of(2018, 3, 1, 11, 30));
    LogbookEntry entry2 = new LogbookEntry("Implementierung",
            LocalDateTime.of(2018, 3, 1, 11, 30), LocalDateTime.of(2018, 3, 1, 16, 30));
    LogbookEntry entry3 = new LogbookEntry("Testen",
            LocalDateTime.of(2018, 3, 1, 10, 15), LocalDateTime.of(2018, 3, 1, 14, 30));

    empl1.addLogbookEntry(entry1);
    empl1.addLogbookEntry(entry2);
    empl2.addLogbookEntry(entry3);

    empl1 = workLog.syncEmployee(empl1);
    empl2 = workLog.syncEmployee(empl2);
  }

  private void findAll(WorkLogService workLog) {
    for (Employee e : workLog.findAllEmployees()) {
      System.out.println(e);
      e.getLogbookEntries().forEach(entry ->
              System.out.println("   " + entry.getId() + ": " + entry));
    }
  }

}
