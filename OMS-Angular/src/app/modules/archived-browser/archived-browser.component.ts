import { Component, OnInit } from '@angular/core';

import { ArchivedOutage } from '@shared/models/outage.model';

export interface PeriodicElement {
  name: string;
  position: number;
  weight: number;
  symbol: string;
};

const AO_MOCK: ArchivedOutage[] = [
  { Id: 1, ElementId: 2321619, ReportedAt: new Date(), AfectedConsumers: [], ArchivedAt: new Date() },
  { Id: 2, ElementId: 3311516, ReportedAt: new Date(), AfectedConsumers: [], ArchivedAt: new Date() },
  { Id: 3, ElementId: 4321512, ReportedAt: new Date(), AfectedConsumers: [], ArchivedAt: new Date() },
  { Id: 4, ElementId: 5684515, ReportedAt: new Date(), AfectedConsumers: [], ArchivedAt: new Date() },
  { Id: 5, ElementId: 6151715, ReportedAt: new Date(), AfectedConsumers: [], ArchivedAt: new Date() },
  { Id: 6, ElementId: 7236541, ReportedAt: new Date(), AfectedConsumers: [], ArchivedAt: new Date() }
];

@Component({
  selector: 'app-archived-browser',
  templateUrl: './archived-browser.component.html',
  styleUrls: ['./archived-browser.component.css']
})

export class ArchivedBrowserComponent implements OnInit {
  private archivedOutages: ArchivedOutage[] = AO_MOCK;
  private columns: string[] = ["id", "elementId", "reportedAt", "archivedAt"];

  constructor() { }

  ngOnInit() {
    console.log(this.archivedOutages);
  }

}

