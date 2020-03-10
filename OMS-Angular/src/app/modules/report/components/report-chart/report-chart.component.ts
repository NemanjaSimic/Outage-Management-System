import { Component, OnInit } from '@angular/core';
import { data }  from './data.mock';

@Component({
  selector: 'app-report-chart',
  templateUrl: './report-chart.component.html',
  styleUrls: ['./report-chart.component.css']
})
export class ReportChartComponent implements OnInit {
  data: any[];

  view: any[] = [700, 400];

  // options
  showXAxis = true;
  showYAxis = true;
  showLegend = true;
  showXAxisLabel = true;
  xAxisLabel = 'Years';
  showYAxisLabel = true;
  yAxisLabel = '# of outages per consumer';

  constructor() {
    Object.assign(this, { data })
   }

  ngOnInit() {
  }

}
