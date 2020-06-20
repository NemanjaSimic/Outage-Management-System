import { Component, OnInit } from '@angular/core';
import { ReportService } from '@services/report/report.service';
import { Report } from '@shared/models/report.model';
import { ChartData } from '@shared/models/chart-data.model';

@Component({
  selector: 'app-report',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
  public hasChartData: Boolean = false;
  public chartData;
  public chartType;

  constructor(private reportService: ReportService) { }

  ngOnInit() {
  }

  onGenerateHandler(options): void {
    this.hasChartData = true;
    console.log(options);

    this.reportService.generateReport(options).subscribe(
      (report: Report)  => {
        const chartData: ChartData[] = [];

        for(const [key, value] of Object.entries(report.Data))
          chartData.push({ name: key, value });

        console.log(chartData);
        this.chartData = chartData;
        this.chartType = report.Type;
      },
      err => console.error(err)
    );
  }

}
