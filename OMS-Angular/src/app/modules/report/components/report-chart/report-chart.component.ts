import { Component, OnInit, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { data }  from './data.mock';

@Component({
  selector: 'app-report-chart',
  templateUrl: './report-chart.component.html',
  styleUrls: ['./report-chart.component.css']
})
export class ReportChartComponent implements OnInit {
  @Input() data: any[];
  @Input() type: string = "Yearly";

  view: any[] = [700, 400];

  // options
  showXAxis = true;
  showYAxis = true;
  showLegend = true;
  showXAxisLabel = true;
  showYAxisLabel = true;
  yAxisLabel = '# of outages per consumer';

  constructor() {}

  ngOnInit() { }
}
