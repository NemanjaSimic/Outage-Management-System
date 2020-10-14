import { Component, OnInit } from '@angular/core';
import { ReportService } from '@services/report/report.service';
import { Report } from '@shared/models/report.model';
import { ChartData } from '@shared/models/chart-data.model';
import { ReportType } from '@shared/models/report-options.model';

@Component({
  selector: 'app-report',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
  public hasChartData: Boolean = false;
  public chartData;
  public chartType;
  public reportType;

  constructor(private reportService: ReportService) { }

  ngOnInit() {
  }

  onGenerateHandler(options): void {
    this.hasChartData = true;
    console.log(options);

    //mock
   /*  const chartData: ChartData[] = [];
    chartData.push({name:'2', value: 2});
    this.chartData = chartData;
    this.chartType = 'Monthly';
    this.reportType = options.Type; */

    this.reportService.generateReport(options).subscribe(
      (report: Report)  => {
        const chartData: ChartData[] = [];

        for(const [key, value] of Object.entries(report.Data))
          chartData.push({ name: key, value });

        console.log(chartData);
        this.chartData = chartData;
        this.chartType = report.Type;
        this.reportType = options.Type;
      },
      err => console.error(err)
    );
  }

}
