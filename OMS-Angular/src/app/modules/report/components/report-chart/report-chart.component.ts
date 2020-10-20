import { Component, OnInit, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { GraphService } from '@services/notification/graph.service';
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

  private numOfConsumers: number = 9512;

  constructor(private graphService: GraphService) {}

  ngOnChanges(changes: SimpleChanges): void {
    this.getYAxisLabel(changes.reportType.currentValue);
  }

  ngOnInit() { 
   //this.getYAxisLabel(this.reportType);

   // @Note: 
   // - bolje je da prosledimo iz report-a ovaj podatak ka chart i form nego da imamo 2 pojedinacna poziva, al deadline..
   // - ovaj deo koda mozda treba iznad u ngOnChanges
   this.graphService.getTopology().subscribe((graph) => {
    this.numOfConsumers = graph.Nodes.filter(node => node.DMSType == "ENERGYCONSUMER").length;
  });

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
