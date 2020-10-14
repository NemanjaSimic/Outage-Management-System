import { Component, OnInit, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { ReportType } from '@shared/models/report-options.model';
import { data }  from './data.mock';

@Component({
  selector: 'app-report-chart',
  templateUrl: './report-chart.component.html',
  styleUrls: ['./report-chart.component.css']
})
export class ReportChartComponent implements OnInit, OnChanges {
  @Input() data: any[];
  @Input() type: string = "Yearly";
  @Input() reportType : string = "1";

  view: any[] = [700, 400];

  // options
  showXAxis = true;
  showYAxis = true;
  showLegend = true;
  showXAxisLabel = true;
  showYAxisLabel = true;
  yAxisLabel = "";
  colorScheme = {
    domain: ['#FFFB33']
  };

  constructor() {}
  ngOnChanges(changes: SimpleChanges): void {
    this.getYAxisLabel(changes.reportType.currentValue);
  }

  ngOnInit() { 
   //this.getYAxisLabel(this.reportType);
  }

  getYAxisLabel(type : any)
  {
    switch(type)
    {
      case "0":
        this.yAxisLabel = '# of outages per consumer';
        break;
      case "2":
        this.yAxisLabel =  'SAIDI';
        break;
      case "1":
        this.yAxisLabel =  'SAIFI';
        break;

    }
  }
}
