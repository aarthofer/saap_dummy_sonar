import {Component, OnInit} from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {EmployeeRestControllerService} from '../api/services/employee-rest-controller.service';
import {LogbookEntryRestControllerService} from '../api/services/logbook-entry-rest-controller.service';
import {LogbookEntryDto} from '../api/models/logbook-entry-dto';
import {EmployeeDto} from '../api/models/employee-dto';
import {HttpErrorResponse} from '@angular/common/http';

@Component({
  selector: 'app-entry-list',
  templateUrl: './entry-list.component.html',
  styleUrls: ['./entry-list.component.css']
})
export class EntryListComponent implements OnInit {

  employee: EmployeeDto | undefined;
  entryList: LogbookEntryDto[] | undefined;
  errorInfo: string = "";

  constructor(private route: ActivatedRoute,
              private employeeService: EmployeeRestControllerService,
              private entryService: LogbookEntryRestControllerService) {
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      // @ts-ignore
      const employeeId = +params.get('employeeId');

      this.employeeService.getEmployeeById({id: employeeId})
        .subscribe(employee => {
            this.employee = employee;
            this.refreshEntryList(employeeId);
            this.resetError();
          },
          (error: HttpErrorResponse) => {
            this.displayError(error);
          });
    });
  }

  refreshEntryList(emplId: number): void {
    this.entryService.getLogbookEntriesForEmployee({employeeId: emplId})
      .subscribe(entries => {
          this.entryList = entries;
          this.resetError();
        },
        (error: HttpErrorResponse) => {
          this.displayError(error);
        });
  }

  deleteEntry(entryId: number): void {
    //TODO implement worklog-api delete function first!
    //entryService.deleteLogbookEntry(...)
  }


  private displayError(resp: HttpErrorResponse): void {
    this.errorInfo = `${resp.error.error} (${resp.error.status}): ${resp.error.message}.`;
  }

  private resetError(): void {
    this.errorInfo = "";
  }
}
