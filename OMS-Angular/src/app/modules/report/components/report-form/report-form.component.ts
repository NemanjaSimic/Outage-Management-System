import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-report-form',
  templateUrl: './report-form.component.html',
  styleUrls: ['./report-form.component.css']
})
export class ReportFormComponent implements OnInit {
  
  reportTypes: any[] = [
    {value: '0', name: 'SAIFI'},
    {value: '1', name: 'SAIDI'},
    {value: '2', name: 'Total'}
  ];

  constructor() { }

  ngOnInit() {
  }

}
