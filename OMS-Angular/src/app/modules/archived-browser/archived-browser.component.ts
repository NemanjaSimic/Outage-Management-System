import { Component, OnInit, OnDestroy } from '@angular/core';

import { ArchivedOutage, OutageLifeCycleState } from '@shared/models/outage.model';
import { OutageService } from '@services/outage/outage.service';
import { Subscription } from 'rxjs';
import { MatDialog } from '@angular/material';
import { ArchivedOutageModalComponent } from '@shared/components/archived-outage-modal/archived-outage-modal.component';

@Component({
  selector: 'app-archived-browser',
  templateUrl: './archived-browser.component.html',
  styleUrls: ['./archived-browser.component.css']
})

export class ArchivedBrowserComponent implements OnInit {
  public archivedOutagesSubscription: Subscription;

  public archivedOutages: ArchivedOutage[] = [];
  public columns: string[] = ["id", "elementId", "reportedAt", "archivedAt", "actions"];

  constructor(private outageService: OutageService, private dialog: MatDialog) { }

  ngOnInit() {

    const outage: ArchivedOutage = {
      AffectedConsumers: [],
      ElementId: 123,
      FixedAt: new Date(),
      Id: 213,
      IsResolveConditionValidated: true,
      IsolatedAt: new Date(),
      RepairedAt: new Date(),
      ReportedAt: new Date(),
      State: OutageLifeCycleState.Archived,
      ArchivedAt: new Date()
    }

    this.archivedOutages.push(outage);
    this.archivedOutages.push(outage);

    this.getArchivedOutages();
  }

  private getArchivedOutages(): void {
    this.outageService.getAllArchivedOutages().subscribe(
      outages => this.archivedOutages = outages, 
      err => console.log(err)
    );
  }

  showMoreDetails(outage: ArchivedOutage) : void{
    this.dialog.open(ArchivedOutageModalComponent, {
       data: outage
     });
  }

  public getOutageStateString(state: any) : string {
    return OutageLifeCycleState[state];
  }

}

