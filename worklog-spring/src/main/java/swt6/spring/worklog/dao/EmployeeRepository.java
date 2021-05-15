package swt6.spring.worklog.dao;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;
import swt6.spring.worklog.domain.Employee;

import java.time.LocalDate;
import java.util.List;
import java.util.Optional;

@Repository
public interface EmployeeRepository extends JpaRepository<Employee, Long> {
    // List<Employee> findAllByFirstNameIn(String name);

    @Query("select e from Employee e where e.lastName like '%:substr%'")
    List<Employee> findByLastNameContaining(@Param("substr") String substr);

    @Query("select e from Employee e where e.dateOfBirth < :date")
    List<Employee> findOlderThan(@Param("date") LocalDate date);

    Optional<Employee> findByLastName(String malden);

    Iterable<Object> findAllByFirstName(String name);

    Iterable<Object> findAllByFirstNameIn(List<String> karl);
}
