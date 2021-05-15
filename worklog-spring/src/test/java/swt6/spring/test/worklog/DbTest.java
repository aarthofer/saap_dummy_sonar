package swt6.spring.test.worklog;


import org.junit.Ignore;
import org.junit.Test;
import org.springframework.context.support.AbstractApplicationContext;
import org.springframework.context.support.ClassPathXmlApplicationContext;
import swt6.spring.worklog.dao.EmployeeDao;
import swt6.spring.worklog.dao.EmployeeRepository;
import swt6.spring.worklog.domain.Employee;
import swt6.util.*;

import javax.persistence.EntityManagerFactory;
import javax.sql.DataSource;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.sql.SQLException;
import java.time.LocalDate;
import java.util.List;
import java.util.Optional;

import static swt6.util.PrintUtil.*;

public class DbTest {

  private void createSchema(DataSource ds, String ddlScript) {
    try {
      DbScriptRunner scriptRunner = new DbScriptRunner(ds.getConnection());
      InputStream is = DbTest.class.getClassLoader().getResourceAsStream(ddlScript);
      if (is == null) throw new IllegalArgumentException(
        String.format("File %s not found in classpath.", ddlScript));
      scriptRunner.runScript(new InputStreamReader(is));
    }
    catch (SQLException | IOException e) {
      e.printStackTrace();
      return;
    }
  }

  @Ignore
  @Test
  public void testCreateJdbcSchema(){
    try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
            "swt6/spring/worklog/test/applicationContext-jdbc.xml")) {
      printTitle("create schema", 60, '-');
      createSchema(factory.getBean("dataSource", DataSource.class),
              "swt6/spring/worklog/test/CreateWorklogDbSchema.sql");
    }
  }


  @Test
  public void testJdbcInsert() {

    try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
            "swt6/spring/worklog/test/applicationContext-jdbc.xml")) {

      printTitle("create schema", 60, '-');
      createSchema(factory.getBean("dataSource", DataSource.class),
              "swt6/spring/worklog/test/CreateWorklogDbSchema.sql");

      EmployeeDao emplDao = factory.getBean("employeeDaoJdbc", EmployeeDao.class);

      printTitle("insert employee", 60, '-');
      Employee empl1 =
              new Employee("John", "Doe", LocalDate.of(1970, 10, 26));
      emplDao.insert(empl1);
      System.out.println("empl1 = " + (empl1 == null ? (null) : empl1.toString()));
    }
  }
  
  @Test
  public void testJdbcUpdate() {

    try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
            "swt6/spring/worklog/test/applicationContext-jdbc.xml")) {

      printTitle("create schema", 60, '-');
      createSchema(factory.getBean("dataSource", DataSource.class),
              "swt6/spring/worklog/test/CreateWorklogDbSchema.sql");

      EmployeeDao emplDao = factory.getBean("employeeDaoJdbc", EmployeeDao.class);

      printTitle("insert employee", 60, '-');
      Employee empl1 =
              new Employee("Charly", "Brown", LocalDate.of(1970, 10, 26));
      emplDao.insert(empl1);
      System.out.println("empl1 = " + (empl1 == null ? (null) : empl1.toString()));

      printTitle("update employee", 60, '-');
      empl1.setFirstName("Charles");
      empl1 = emplDao.merge(empl1);
      System.out.println("empl1 = " + (empl1 == null ? (null) : empl1.toString()));
    }
  }


  @Test
  public void testJdbcInsertUpdateFind() {

    try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
      "swt6/spring/worklog/test/applicationContext-jdbc.xml")) {

      printTitle("create schema", 60, '-');
      createSchema(factory.getBean("dataSource", DataSource.class),
        "swt6/spring/worklog/test/CreateWorklogDbSchema.sql");

      EmployeeDao emplDao = factory.getBean("employeeDaoJdbc", EmployeeDao.class);

      printTitle("insert employee", 60, '-');
      Employee empl1 =
        new Employee("Josefine", "Feichtlbauer", LocalDate.of(1970, 10, 26));
      emplDao.insert(empl1);
      System.out.println("empl1 = " + (empl1 == null ? (null) : empl1.toString()));

      printTitle("update employee", 60, '-');
      empl1.setFirstName("Jaquira");
      empl1 = emplDao.merge(empl1);
      System.out.println("empl1 = " + (empl1 == null ? (null) : empl1.toString()));

      printTitle("find employee", 60, '-');
      Employee empl = emplDao.findById(1L);
      System.out.println("empl = " + (empl == null ? (null) : empl.toString()));
      empl = emplDao.findById(100L);
      System.out.println("empl = " + (empl == null ? (null) : empl.toString()));

      printTitle("find all employees", 60, '-');
      emplDao.findAll().forEach(System.out::println);
    }
  }

  @Test
  public void testJpaInsert() {
    try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
            "swt6/spring/worklog/test/applicationContext-jpa.xml")) {
      printTitle("insert employee", 60, '-');
      var employeeDao = factory.getBean("employeeDaoJpa", EmployeeDao.class);
      var employee =
              new Employee("Jane", "Doe", LocalDate.of(1950, 1, 1));

      EntityManagerFactory emFactory = factory.getBean( EntityManagerFactory.class );

      JpaUtil.beginTransaction( emFactory );
      employeeDao.insert(employee);
      System.out.printf("inserted employee: %s %n ",employee.toString());
      JpaUtil.commitTransaction( emFactory );
    }
  }

  @Test
  public void testJpa() {
    try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
            "swt6/spring/worklog/test/applicationContext-jpa.xml")) {

        EntityManagerFactory emFactory = factory.getBean(EntityManagerFactory.class);
        EmployeeDao emplDao = factory.getBean("employeeDaoJpa", EmployeeDao.class);

        Employee empl1 =
                new Employee("Josef", "Himmelbauer", LocalDate.of(1950, 1, 1));

        printTitle("insert employee", 60, '-');
        try {
            JpaUtil.beginTransaction(emFactory);
            emplDao.insert(empl1);
        } finally {
            JpaUtil.commitTransaction(emFactory);
        }

        printTitle("find employee", 60, '-');
        final Long empl1Id = empl1.getId();
        JpaUtil.executeInTransaction(emFactory, () -> {
            Employee empl = emplDao.findById(empl1Id);
            System.out.println("empl=" + (empl == null ? (null) : empl.toString()));
            empl = emplDao.findById(100L);
            System.out.println("empl=" + (empl == null ? (null) : empl.toString()));
        });

        printTitle("update employee", 60, '-');
        try {
            JpaUtil.beginTransaction(emFactory);
            empl1 = emplDao.merge(empl1);
            empl1.setLastName("Himmelbauer-Schmidt");
        } finally {
            JpaUtil.commitTransaction(emFactory);
        }

        printTitle("find all employees", 60, '-');
        JpaUtil.executeInTransaction(emFactory, () -> {
            System.out.println("findAll");
            emplDao.findAll().forEach(System.out::println);
        });
    }
  }

  @Test
  public void testSpringData() {
      try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
              "swt6/spring/worklog/test/applicationContext-data.xml")) {

          EntityManagerFactory emFactory = factory.getBean(EntityManagerFactory.class);

          JpaUtil.executeInTransaction(emFactory, () -> {
              EmployeeRepository emplRepo =
                      JpaUtil.getJpaRepository(emFactory, EmployeeRepository.class);

              Employee empl1 =
                      new Employee("Josef", "Himmelbauer", LocalDate.of(1950, 1, 1));
              Employee empl2 = new Employee("Karl", "Malden", LocalDate.of(1940, 5, 3));

              printTitle("insert employee", 60, '-');
              empl1 = emplRepo.save(empl1);
              empl2 = emplRepo.save(empl2);
              emplRepo.flush();

              printTitle("update employee", 60, '-');
              empl1.setLastName("Himmelbauer-Huber");
              empl1 = emplRepo.save(empl1);
          });

          printSeparator(60, '-');

          JpaUtil.executeInTransaction(emFactory, () -> {
              EmployeeRepository emplRepo =
                      JpaUtil.getJpaRepository(emFactory, EmployeeRepository.class);

              printTitle("findById", 60, '-');
              Optional<Employee> empl1 = emplRepo.findById(1L);
              System.out.println("empl=" + (empl1.isPresent() ? empl1.get()
                      : "<not-found>"));

              printTitle("findAll", 60, '-');
              emplRepo.findAll().forEach(System.out::println);
          });

          printSeparator(60, '-');

          JpaUtil.executeInTransaction(emFactory, () -> {
              EmployeeRepository emplRepo =
                      JpaUtil.getJpaRepository(emFactory, EmployeeRepository.class);

              printTitle("findByLastName", 60, '-');
              Optional<Employee> empl2 = emplRepo.findByLastName("Malden");
              System.out.println(
              "empl=" + (empl2.isPresent() ? empl2.get().toString() : "<not-found>"));

              printTitle("findByLastNameContaining", 60, '-');
              emplRepo.findByLastNameContaining("a").forEach(System.out::println);

              printTitle("findOlderThan", 60, '-');
              emplRepo.findOlderThan(LocalDate.of(1948, 1, 1))
                      .forEach(System.out::println);

              printTitle( "findFirstNameIn", 60, '-' );
              emplRepo.findAllByFirstNameIn(List.of("Karl") ).forEach( System.out::println );

              printTitle( "findLastNameContaining", 60, '-' );
              Optional<Employee> optEmployee = emplRepo.findEmployeeByLastNameContaining( "Ma" );
              System.out.println( optEmployee.get() );
          });
      }
  }

}
