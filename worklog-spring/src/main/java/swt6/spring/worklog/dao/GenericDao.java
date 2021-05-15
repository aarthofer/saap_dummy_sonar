package swt6.spring.worklog.dao;

import swt6.spring.worklog.domain.Employee;

import java.util.List;

public interface GenericDao<E,ID> {

    E findById(ID id);
    List<E> findAll();
    void insert(E e);
    E merge(E e);

}
