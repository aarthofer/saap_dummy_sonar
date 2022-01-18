package at.arthofer.fh.wea5.shop;

import java.lang.reflect.Constructor;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.servlet.ServletException;

import at.arthofer.fh.wea5.shop.warehouse.ShopDelegate;

/*
* The ServiceLocator class provides access to all relevant services
* (e.g. database) and it offers the appropriate delegate object. 
* The serviceLocator is based on the singleton pattern.
*/
public class ServiceLocator {

	private static Logger logger = Logger.getLogger("WEA5Bookshop");

	private static ServiceLocator instance;
	private boolean initialized;

	private String dbDsn;
	private String dbUsername;
	private String dbPassword;
	private String delegateClass;

	private ServiceLocator() {
		// Intentionally private: Is a Singleton.
		initialized = false;
	}

	public static synchronized ServiceLocator getInstance() {
		if (instance == null) {
			instance = new ServiceLocator();
		}
		return instance;
	}

	public void init(String dbDsn, String dbUsername, String dbPassword, String delegateClass) {
		this.dbDsn = dbDsn;
		this.dbUsername = dbUsername;
		this.dbPassword = dbPassword;
		this.delegateClass = delegateClass;
	}

	public Connection getDBConnection() throws ServletException, ClassNotFoundException {
		if (!initialized) {
			// Class.forName("sun.jdbc.odbc.JdbcOdbcDriver");
			Class.forName("org.apache.derby.jdbc.EmbeddedDriver");
			// Class.forName("org.apache.derby.jdbc.ClientDriver");
			logger.log(Level.INFO, "getDBConnection: loading driver class ... ");
			this.initialized = true;
		}
		try {
			logger.log(Level.INFO, "connecting to " + dbDsn);
			return DriverManager.getConnection(dbDsn, dbUsername, dbPassword);
		} catch (SQLException e) {
			logger.log(Level.SEVERE, "getDBConnection: " + e);
			throw new ServletException(e);
		}
	}

	/*
	 * Get the delegate object, which has been defined in the web.xml file The
	 * delegate object is created with the Java reflection mechanism.
	 */
	public ShopDelegate getShopDelegate() throws ServletException {
		Class<?> cls;
		try {
			cls = Class.forName(this.delegateClass);
			Constructor<?> c = cls.getConstructor();
			ShopDelegate delegate = (ShopDelegate) c.newInstance();
			return delegate;
		} catch (Exception e) {
			logger.log(Level.SEVERE, "ServiceLocator: " + e, e);
		}
		return null;
	}
}
