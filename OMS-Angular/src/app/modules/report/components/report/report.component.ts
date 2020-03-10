import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-report',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
  public hasChartData: Boolean = false;

  constructor() { }

  ngOnInit() {
  }

  onGenerateHandler(data): void {
    this.hasChartData = true;
  }

}
