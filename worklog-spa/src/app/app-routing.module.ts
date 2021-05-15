import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {EmployeeListComponent} from "./employee-list/employee-list.component";
import {EntryListComponent} from "./entry-list/entry-list.component";

const routes: Routes = [
  {path:'', redirectTo: 'employees', pathMatch: 'full'},
  {path:'employees', component: EmployeeListComponent},
  {path:'employees/:employeeId/entries', component: EntryListComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
