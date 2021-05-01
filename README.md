# swt6b-spring
swt6b repo for exercise 5,  6 and 7 (Dependency Injection, AOP, Spring)


## project structure

```
projects-spring
|
+--intro-spring
   |
   +-- swt6/spring/basics
       |
       +-- ioc
       |
       +-- apo
```

### intro-spring.ioc: Dependency Injection

![Logger](/doc/logger-problem.png)

![Logger Factory Based](/doc/logger-factory-solution.png)

![Logger Dependeny Injection Based](/doc/logger-di-solution.png)

### intro-spring.ioc; Annotations (javax.inject.*, import org.springframework.stereotype.*)

```
┌────────────┬─────────────────────────────────────────────────────┐
│ Annotation │ Meaning                                             │
├────────────┼─────────────────────────────────────────────────────┤
│ @Component │ generic stereotype for any Spring-managed component │
│ @Repository│ stereotype for persistence layer                    │
│ @Service   │ stereotype for service layer                        │
│ @Controller│ stereotype for presentation layer (spring-mvc)      │
└────────────┴─────────────────────────────────────────────────────┘
```

