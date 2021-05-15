/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { BaseService } from '../base-service';
import { ApiConfiguration } from '../api-configuration';
import { StrictHttpResponse } from '../strict-http-response';
import { RequestBuilder } from '../request-builder';
import { Observable } from 'rxjs';
import { map, filter } from 'rxjs/operators';

import { LogbookEntryDto } from '../models/logbook-entry-dto';

@Injectable({
  providedIn: 'root',
})
export class LogbookEntryRestControllerService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation getLogbookEntriesForEmployee
   */
  static readonly GetLogbookEntriesForEmployeePath = '/worklog/entries';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `getLogbookEntriesForEmployee()` instead.
   *
   * This method doesn't expect any request body.
   */
  getLogbookEntriesForEmployee$Response(params: {
    employeeId: number;
  }): Observable<StrictHttpResponse<Array<LogbookEntryDto>>> {

    const rb = new RequestBuilder(this.rootUrl, LogbookEntryRestControllerService.GetLogbookEntriesForEmployeePath, 'get');
    if (params) {
      rb.query('employeeId', params.employeeId, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'application/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<LogbookEntryDto>>;
      })
    );
  }

  /**
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `getLogbookEntriesForEmployee$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  getLogbookEntriesForEmployee(params: {
    employeeId: number;
  }): Observable<Array<LogbookEntryDto>> {

    return this.getLogbookEntriesForEmployee$Response(params).pipe(
      map((r: StrictHttpResponse<Array<LogbookEntryDto>>) => r.body as Array<LogbookEntryDto>)
    );
  }

}
