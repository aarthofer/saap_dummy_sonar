import {Component, OnInit} from '@angular/core';
import {EmployeeRestControllerService} from '../api/services';
import {EmployeeDto} from '../api/models';
import {HttpErrorResponse} from '@angular/common/http';

@Component({
  selector: 'app-employee-list',
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.css']
})
export class EmployeeListComponent implements OnInit {

  employeeList: EmployeeDto[] = [];
  errorInfo: string = "";

  constructor(private employeeService: EmployeeRestControllerService) { }

  ngOnInit(): void {

    // TODO implement me!
    
    /*
    this.employeeService.getEmployees()
      .subscribe(employees => {
          this.employeeList = employees;
          this.resetError();
        },
        (error: HttpErrorResponse) => {
          this.displayError(error);
        });
     */
  }

  private displayError(resp: HttpErrorResponse): void {
    this.errorInfo = `${resp.error.error} (${resp.error.status}): ${resp.error.message}.`;
  }

  private resetError(): void {
    this.errorInfo = "";
  }
}
