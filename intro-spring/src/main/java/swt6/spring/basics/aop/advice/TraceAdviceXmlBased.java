package swt6.spring.basics.aop.advice;

import org.aspectj.lang.JoinPoint;
import org.aspectj.lang.ProceedingJoinPoint;

public class TraceAdviceXmlBased {


        public void traceBefore( JoinPoint jp ) {
            String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
            System.out.println( "--> " + methodName );
        }

        public void traceAfter( JoinPoint jp ) {
            String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
            System.out.println( "<-- " + methodName );
        }

        public Object traceAround(ProceedingJoinPoint jp ) throws Throwable {
            String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
            System.out.println( "==> " + methodName );
            Object retVal = jp.proceed();  // executes advised method
            System.out.println( "<== " + methodName );
            return retVal;
        }

        public void traceException( JoinPoint jp, Throwable exception ) {
            String methodName = jp.getTarget().getClass().getName() + "." + jp.getSignature().getName();
            System.out.printf( "##>  %s%n threw exception <%s>%n", methodName, exception );
        }


}
