import { Component, OnInit } from '@angular/core';

import { ActiveOutage } from '@shared/models/outage.model';

export interface PeriodicElement {
  name: string;
  position: number;
  weight: number;
  symbol: string;
};

const AO_MOCK: ActiveOutage[] = [
  { Id: 1, ElementId: 2321619, DateCreated: new Date(), AfectedConsumers: [] },
  { Id: 2, ElementId: 3311516, DateCreated: new Date(), AfectedConsumers: [] },
  { Id: 3, ElementId: 4321512, DateCreated: new Date(), AfectedConsumers: [] },
  { Id: 4, ElementId: 5684515, DateCreated: new Date(), AfectedConsumers: [] },
  { Id: 5, ElementId: 6151715, DateCreated: new Date(), AfectedConsumers: [] },
  { Id: 6, ElementId: 7236541, DateCreated: new Date(), AfectedConsumers: [] }
];

@Component({
  selector: 'app-active-browser',
  templateUrl: './active-browser.component.html',
  styleUrls: ['./active-browser.component.css']
})

export class ActiveBrowserComponent implements OnInit {
  private activeOutages: ActiveOutage[] = AO_MOCK;
  private columns: string[] = ["id", "elementId", "dateCreated"];

  constructor() { }

  ngOnInit() {
    console.log(this.activeOutages);
  }

}

