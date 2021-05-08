package swt6.spring.basics.ioc.util;

import java.io.IOException;
import java.util.Properties;

public class LoggerFactory {

    public static Logger getLogger() {
        if ("file".equals(getLoggerType())) {
            return new FileLogger("log.txt");
        } else {
            return new ConsoleLogger();
        }
    }

    private static String getLoggerType() {
        Properties properties = new Properties();
        try {
            ClassLoader cl = LoggerFactory.class.getClassLoader();
            properties.load(cl.getResourceAsStream("swt6/spring/basics/worklog.properties"));
        } catch (IOException ex) {
            System.err.println("file worklog.properties not found using consoleLogger per default!");
        }
        return properties.getProperty("loggerType", "console");
    }
}
