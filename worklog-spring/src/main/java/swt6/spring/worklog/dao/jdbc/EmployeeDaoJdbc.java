package swt6.spring.worklog.dao.jdbc;

import org.springframework.dao.DataAccessException;
import org.springframework.dao.IncorrectResultSizeDataAccessException;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.jdbc.core.RowMapper;
import org.springframework.jdbc.support.GeneratedKeyHolder;
import org.springframework.jdbc.support.KeyHolder;
import swt6.spring.worklog.dao.EmployeeDao;
import swt6.spring.worklog.domain.Employee;

import java.sql.*;
import java.util.List;
import java.util.Map;

public class EmployeeDaoJdbc implements EmployeeDao {

    private JdbcTemplate jdbcTemplate;

    public void setJdbcTemplate(JdbcTemplate jdbcTemplate) {
        this.jdbcTemplate = jdbcTemplate;
    }

    protected static class EmployeeRowMapper implements RowMapper<Employee> {

        @Override
        public Employee mapRow(ResultSet resultSet, int i) throws SQLException {
            Employee employee = new Employee();
            employee.setId( resultSet.getLong(1) );
            employee.setFirstName( resultSet.getString( 2 ) );
            employee.setLastName( resultSet.getString( 3 ) );
            employee.setDateOfBirth( resultSet.getDate(4).toLocalDate() );
            return employee;
        }
    }


    @Override
    public Employee findById(Long id ) throws DataAccessException {
        final String sql = "SELECT ID, FIRSTNAME, LASTNAME, DATEOFBIRTH FROM EMPLOYEE WHERE ID=?";
        List<Employee> employees = jdbcTemplate.query( sql, new EmployeeRowMapper(), id);
        if( employees.size() == 0 )
            return null;
        else if( employees.size() == 1 ) {
            return employees.get( 0 );
        } else
            throw new IncorrectResultSizeDataAccessException(1, employees.size() );
    }

    @Override
    public List<Employee> findAll() throws DataAccessException {
        final String sql = "SELECT ID, FIRSTNAME, LASTNAME, DATEOFBIRTH FROM EMPLOYEE";
        return jdbcTemplate.query( sql, new EmployeeRowMapper() );
    }

    @Override
    public void insert(Employee employee) throws DataAccessException {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) VALUES (?,?,?)";
        KeyHolder keyHolder = new GeneratedKeyHolder();
        jdbcTemplate.update( conn -> {
            PreparedStatement ps = conn.prepareStatement( sql, new String[] {"id"} );
            ps.setString( 1, employee.getFirstName() );
            ps.setString( 2, employee.getLastName() );
            ps.setDate( 3, Date.valueOf( employee.getDateOfBirth() ) );
            return ps;
        }, keyHolder  );
        employee.setId( keyHolder.getKey().longValue() );
    }

    public void insert_v3(Employee employee) {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) VALUES (?,?,?)";
        jdbcTemplate.update(sql, employee.getFirstName(), employee.getLastName(), Date.valueOf(employee.getDateOfBirth()));
    }

    public void insert_v2(Employee employee) {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) VALUES (?,?,?)";
        jdbcTemplate.update( sql, ps -> {
            ps.setString( 1, employee.getFirstName() );
            ps.setString( 2, employee.getLastName() );
            ps.setDate( 3, Date.valueOf( employee.getDateOfBirth() ) );
        } );
    }


    public void insert_v1(Employee employee) {
        final String sql = "INSERT INTO EMPLOYEE (FIRSTNAME, LASTNAME, DATEOFBIRTH) VALUES (?,?,?)";
        try(Connection  conn = jdbcTemplate.getDataSource().getConnection();
            PreparedStatement stmt = conn.prepareStatement( sql )) {
            stmt.setString(1, employee.getFirstName());
            stmt.setString( 2, employee.getLastName() );
            stmt.setDate( 3, Date.valueOf( employee.getDateOfBirth() ));
            stmt.executeUpdate();
        } catch( SQLException e ) {
            System.err.println( e );
        }
    }

    @Override
    public Employee merge(Employee employee) throws DataAccessException {
        if( employee.getId() == null ) {
            insert( employee );
        } else {
            update( employee );
        }
        return employee;
    }

    private void update( Employee employee ) {
        final String sql = "UPDATE EMPLOYEE SET FIRSTNAME=?, LASTNAME=?, DATEOFBIRTH=? WHERE ID=?";
        jdbcTemplate.update(sql,
                employee.getFirstName(),
                employee.getLastName(),
                Date.valueOf(employee.getDateOfBirth()),
                employee.getId() );
    }







}
