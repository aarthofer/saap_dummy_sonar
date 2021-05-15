package swt6.spring.basics.aop.advice;

import org.aspectj.lang.JoinPoint;
import org.aspectj.lang.ProceedingJoinPoint;
import org.aspectj.lang.annotation.*;
import org.springframework.stereotype.Component;

@Aspect
@Component
public class TraceAdviceAnnotationBased {


    @Pointcut( "execution(public * swt6.spring.basics.aop.logic..*find*(..))" )
    private void findMethods(){
        // nothing here
    }

    @Around( "execution(public * swt6.spring.basics.aop.logic..*find*ById*(..))")
    public Object traceAround(ProceedingJoinPoint jp ) throws Throwable {
        String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
        System.out.println( "==> " + methodName );
        Object retVal = jp.proceed();  // executes advised method
        System.out.println( "<== " + methodName );
        return retVal;
    }

    // version 1: reference existing pointcut
    //@Before("findMethods()")
    // version 2: define new Pointcut expression
    @Before( "execution(public * swt6.spring.basics.aop.logic..*find*(..))" )
    public void traceBefore( JoinPoint jp ) {
        String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
        System.out.println( "--> " + methodName );
    }

    @AfterReturning( "swt6.spring.basics.aop.advice.TraceAdviceAnnotationBased.findMethods()" )
    public void traceAfter( JoinPoint jp ) {
        String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
        System.out.println( "<-- " + methodName );
    }

    @AfterThrowing( pointcut = "findMethods()", throwing = "exception")
    public void traceException( JoinPoint jp, Throwable exception ) {
        String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
        System.out.printf( "##>  %s%n threw exception <%s>%n", methodName, exception );
    }


}
