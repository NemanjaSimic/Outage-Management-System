import { Component, OnInit } from '@angular/core';

import { ActiveOutage } from '@shared/models/outage.model';
import { OutageService } from '@services/outage/outage.service';

@Component({
  selector: 'app-active-browser',
  templateUrl: './active-browser.component.html',
  styleUrls: ['./active-browser.component.css']
})

export class ActiveBrowserComponent implements OnInit {
  private activeOutages: ActiveOutage[];
  private columns: string[] = ["id", "elementId", "reportedAt"];

  constructor(private outageService: OutageService) { }

  ngOnInit() {
    this.activeOutages = [];
    this.GetActiveOutages();
  }

  private GetActiveOutages(): void {
    this.outageService.getAllActiveOutages().subscribe(
      outages => this.activeOutages = outages, 
      err => console.log(err)
    );

    console.log(this.activeOutages);
  }

}

