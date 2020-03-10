import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { ReportOptions } from '@shared/models/report-options.model';

@Component({
  selector: 'app-report-form',
  templateUrl: './report-form.component.html',
  styleUrls: ['./report-form.component.css']
})
export class ReportFormComponent implements OnInit {

  // @TODO:
  // - fetch from backend
  reportTypes: any[] = [
    { value: '0', name: 'Total' },
    { value: '1', name: 'SAIFI' },
    { value: '2', name: 'SAIDI' },
  ];

  scopes: any[] = [
    { value: '0', name: 'All network elements', gid: 'No GID' },
    { value: '1', name: 'BR_01', gid: '0x00000B00001' },
    { value: '2', name: 'BR_02', gid: '0x00000B00002' },
    { value: '3', name: 'BR_03', gid: '0x00000B00003' },
    { value: '4', name: 'BR_04', gid: '0x00000B00004' },
  ];

  public selectedReportType;
  public filteredScopes: Observable<any[]>;
  public selectedScopeControl = new FormControl();
  public startDate = new FormControl();
  public endDate = new FormControl();

  @Output() generate = new EventEmitter<ReportOptions>();

  constructor() { }

  ngOnInit() {
    this.filteredScopes = this.selectedScopeControl.valueChanges
      .pipe(
        startWith(''),
        map(value => typeof value === 'string' ? value : value.name),
        map(name => name ? this.filterScopes(name) : this.scopes.slice())
      );
  }

  filterScopes(name): any[] {
    const filterValue = name.toLowerCase();
    return this.scopes.filter(option => option.name.toLowerCase().indexOf(filterValue) === 0);
  }

  onSubmitHandler(): void {
    const options: ReportOptions = {
      Type: this.selectedReportType,
      ElementId: this.selectedScopeControl.value,
      StartDate: this.startDate.value,
      EndDate: this.endDate.value
    }

    this.generate.emit(options);
  }


}
