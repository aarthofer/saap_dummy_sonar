package swt6.spring.worklog.dao.jdbc;

import org.springframework.dao.IncorrectResultSizeDataAccessException;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.jdbc.core.RowMapper;
import org.springframework.jdbc.support.GeneratedKeyHolder;
import org.springframework.jdbc.support.KeyHolder;
import swt6.spring.worklog.dao.EmployeeDao;
import swt6.spring.worklog.domain.Employee;

import java.sql.*;
import java.util.List;

public class EmployeeDaoJdbc implements EmployeeDao {

    private JdbcTemplate jdbcTemplate;

    public void setJdbcTemplate(JdbcTemplate jdbcTemplate) {
        this.jdbcTemplate = jdbcTemplate;
    }

    @Override
    public Employee findById(Long aLong) {
        final String sql = "SELECT ID, FIRSTNAME, LASTNAME, DATEOFBIRTH FROM EMPLOYEE WHERE ID = ?";
        List<Employee> result = jdbcTemplate.query(sql, new EmpoyeeRowMapper(), aLong);
        if (result.isEmpty()) {
//            throw new EntityNotFoundException("Employee not found");
            return null;
        } else if (result.size() > 1) {
            throw new IncorrectResultSizeDataAccessException(result.size());
        }
        return result.get(0);
    }

    @Override
    public List<Employee> findAll() {
        final String sql = "SELECT ID, FIRSTNAME, LASTNAME, DATEOFBIRTH FROM EMPLOYEE";
        return jdbcTemplate.query(sql, new EmpoyeeRowMapper());
    }

    @Override
    public void insert(Employee employee) {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) values (?, ?, ?)";
        KeyHolder keyHolder = new GeneratedKeyHolder();
        jdbcTemplate.update(conn -> {
            PreparedStatement stmt = conn.prepareStatement(sql, new String[]{"id"});
            stmt.setString(1, employee.getFirstName());
            stmt.setString(2, employee.getLastName());
            stmt.setDate(3, Date.valueOf(employee.getDateOfBirth()));
            return stmt;
        }, keyHolder);
        employee.setId(keyHolder.getKey().longValue());
    }

    //@Override
    public void insert_v3(Employee employee) {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) values (?, ?, ?)";
        jdbcTemplate.update(sql, employee.getFirstName(), employee.getLastName(), Date.valueOf(employee.getDateOfBirth()));
    }

    //@Override
    public void insert_v2(Employee employee) {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) values (?, ?, ?)";
        jdbcTemplate.update(sql, (ps) -> {
            ps.setString(1, employee.getFirstName());
            ps.setString(2, employee.getLastName());
            ps.setDate(3, Date.valueOf(employee.getDateOfBirth()));
        });
    }

    //@Override
    public void insert_v1(Employee employee) {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) values (?, ?, ?)";
        try (Connection conn = jdbcTemplate.getDataSource().getConnection()) {
            PreparedStatement stmt = conn.prepareStatement(sql);
            stmt.setString(1, employee.getFirstName());
            stmt.setString(2, employee.getLastName());
            stmt.setDate(3, Date.valueOf(employee.getDateOfBirth()));
            stmt.executeUpdate();
        } catch (SQLException e) {
            System.err.println(e);
        }
    }

    @Override
    public Employee merge(Employee employee) {
        if (employee.getId() == null) {
            insert(employee);
        } else {
            update(employee);
        }
        return employee;
    }

    private void update(Employee employee) {
        final String sql = "UPDATE EMPLOYEE SET FIRSTNAME = ?,  LASTNAME = ?, DATEOFBIRTH = ?";
        jdbcTemplate.update(sql, employee.getFirstName(), employee.getLastName(), Date.valueOf(employee.getDateOfBirth()));
    }

    protected static class EmpoyeeRowMapper implements RowMapper<Employee> {

        @Override
        public Employee mapRow(ResultSet resultSet, int i) throws SQLException {
            Employee e = new Employee();
            e.setId(resultSet.getLong(1));
            e.setFirstName(resultSet.getString(2));
            e.setFirstName(resultSet.getString(3));
            e.setDateOfBirth(resultSet.getDate(4).toLocalDate());
            return e;
        }
    }
}
