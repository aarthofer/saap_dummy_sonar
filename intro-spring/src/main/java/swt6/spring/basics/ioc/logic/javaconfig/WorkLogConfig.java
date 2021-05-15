package swt6.spring.basics.ioc.logic.javaconfig;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.ComponentScan;
import org.springframework.context.annotation.Configuration;
import swt6.spring.basics.ioc.logic.WorkLogService;
import swt6.spring.basics.ioc.util.ConsoleLogger;
import swt6.spring.basics.ioc.util.FileLogger;
import swt6.spring.basics.ioc.util.Log;
import swt6.spring.basics.ioc.util.Logger;

@Configuration
@ComponentScan( basePackageClasses = {WorkLogConfig.class, Logger.class})
public class WorkLogConfig {


    /*@Bean
    @Log(Log.Type.STANDARD)
    public Logger consoleLogger() {
        return new ConsoleLogger();
    }

    @Bean
    @Log(Log.Type.FILE)
    public Logger fileLogger() {
        return new FileLogger();
    }*/

    @Bean
    public WorkLogService workLog() {
        //return new WorkLogServiceImpl( consoleLogger() );
        return new WorkLogServiceImpl();
    }

}

