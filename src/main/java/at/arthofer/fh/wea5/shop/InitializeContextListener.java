package at.arthofer.fh.wea5.shop;

import javax.servlet.ServletContext;
import javax.servlet.ServletContextEvent;
import javax.servlet.ServletContextListener;

public class InitializeContextListener implements ServletContextListener {

	public void contextInitialized(ServletContextEvent contextEvent) {
		ServletContext sc = contextEvent.getServletContext();
		String dsn = sc.getInitParameter("DB_DSN");
		String user = sc.getInitParameter("DB_USER");
		String password = sc.getInitParameter("DB_PASSWORD");
		String delegateClass = sc.getInitParameter("SHOP_DELEGATE");

		ServiceLocator.getInstance().init(dsn, user, password, delegateClass);
	}

	public void contextDestroyed(ServletContextEvent servletContextEvent) {
		// Nothing to do.
	}

}
