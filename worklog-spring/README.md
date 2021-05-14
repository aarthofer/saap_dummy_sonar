# worklog-spring

## project structure

```
worklog-spring
└─ swt6 
│  └─ spring.worklog  
│  │  └─ dao
│  │  └─ domain
│  │  └─ logic
│  │  └─ ui 
|  └─ util
└─ swt6.spring.test.worklog
   └─ DBTest
   └─ LogicTest
   └─ UITest
```

## derby maven plugin

- **derby:run** 	starts derby db until you call derby:stop
- **derby:start** 	starts derby and automatically terminates when build is finished 
- **derby:stop** 	stops the derby db
- **derby:drop-db**	deletes the db located in the derbyHome configuration element

## project architecture

![layered architecture](./doc/layered_architecture.png)
