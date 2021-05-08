package swt6.spring.basics.aop;


import org.junit.Ignore;
import org.junit.Test;
import org.springframework.context.support.AbstractApplicationContext;
import org.springframework.context.support.ClassPathXmlApplicationContext;
import swt6.spring.basics.aop.logic.EmployeeIdNotFoundException;
import swt6.spring.basics.aop.logic.WorkLogService;


import java.lang.reflect.Method;

public class AopTest {

    private void reflectClass(Class<?> clazz) {
        System.out.println("class=" + clazz.getName());
        Class<?>[] interfaces = clazz.getInterfaces();
        for (Class<?> itf : interfaces)
            System.out.println("  implements " + itf.getName());

        System.out.println("methods:");
        for (Method m : clazz.getMethods()) {
            System.out.println("  " + m.getName() + "()");
        }
    }

    private void testAOP(String configFileName) {
        try (AbstractApplicationContext appContext =
                     new ClassPathXmlApplicationContext(configFileName)) {

            WorkLogService workLog = appContext.getBean("workLog", WorkLogService.class);
            //reflectClass(workLog.getClass());

            try {
                System.out.printf("%n++ about to call findAllEmployees() ... %n");
                var employeeList = workLog.findAllEmployees();
                System.out.printf("++ ... returned %d nr of employees.%n%n", employeeList.size());

                for (int i = 1; i <= 4; i++)
                    System.out.println("++ "+workLog.findEmployeeById(Long.valueOf(i)) +"\n");
            } catch (EmployeeIdNotFoundException e) {
            }

        }
    }


    @Ignore
    @Test
    public void annotationConfiguredAopTest(){
        System.out.println("testAOP (annotation config based)");
        testAOP("swt6/spring/basics/aop/applicationContext-annotation-config.xml");
    }

    //@Ignore
    @Test
    public void xmlConfiguredAopTest(){
        System.out.println("testAOP (xml config based)");
        testAOP("swt6/spring/basics/aop/applicationContext-xml-config.xml");
    }
}
