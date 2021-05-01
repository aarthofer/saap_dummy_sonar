package swt6.spring.basics.ioc;

import org.junit.Ignore;
import org.junit.Test;

import static org.junit.Assert.assertSame;

import org.springframework.context.annotation.AnnotationConfigApplicationContext;
import org.springframework.context.support.AbstractApplicationContext;
import org.springframework.context.support.ClassPathXmlApplicationContext;
import swt6.spring.basics.ioc.logic.WorkLogService;
import swt6.spring.basics.ioc.logic.factorybased.WorkLogServiceImpl;
import swt6.spring.basics.ioc.logic.javaconfig.WorkLogConfig;


public class IocTest {

    @Test
    public void simpleTest() {
        WorkLogService workLog = new WorkLogServiceImpl();
        workLog.findAllEmployees();
        var employee = workLog.findEmployeeById(3L);
        assertSame(employee.getId(), 3L);

    }

    @Ignore
    @Test
    public void xmlConfigTest() {
        try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
                "swt6/spring/basics/ioc/test/applicationContext-xml-config.xml")) {

            System.out.println("***> workLog-setter-injected:");
            WorkLogService workLog1 = factory.getBean("workLog-setter-injected", WorkLogService.class);
            workLog1.findAllEmployees();
            workLog1.findEmployeeById(3L);

            System.out.println("***> workLog-constructor-injected:");
            WorkLogService workLog2 = factory.getBean("workLog-constructor-injected", WorkLogService.class);
            workLog2.findAllEmployees();
        }
    }

    @Ignore
    @Test
    public void annotationConfigTest() {
        try (AbstractApplicationContext factory = new ClassPathXmlApplicationContext(
                "swt6/spring/basics/ioc/test/applicationContext-annotation-config.xml")) {

            WorkLogService workLog = factory.getBean("workLog", WorkLogService.class);
            workLog.findAllEmployees();
            workLog.findEmployeeById(3L);
        }
    }

    @Ignore
    @Test
    public void javaConfigTest() {
        try (AbstractApplicationContext factory =
                     new AnnotationConfigApplicationContext(WorkLogConfig.class)) {

            WorkLogService workLog = factory.getBean(WorkLogService.class);
            workLog.findAllEmployees();
            workLog.findEmployeeById(3L);
        }
    }

}
