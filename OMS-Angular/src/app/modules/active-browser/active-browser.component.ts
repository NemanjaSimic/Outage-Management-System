import { Component, OnInit } from '@angular/core';

import { ActiveOutage, OutageLifeCycleState } from '@shared/models/outage.model';
import { OutageService } from '@services/outage/outage.service';

import { MatDialog } from '@angular/material';
import { ActiveOutageModalComponent } from '@shared/components/active-outage-modal/active-outage-modal.component';

@Component({
  selector: 'app-active-browser',
  templateUrl: './active-browser.component.html',
  styleUrls: ['./active-browser.component.css']
})

export class ActiveBrowserComponent implements OnInit {
  private activeOutages: ActiveOutage[];
  private columns: string[] = ["id", "elementId", "state", "reportedAt", "moreDetails"];

  constructor(private dialog: MatDialog, private outageService: OutageService) { }

  ngOnInit() {
    this.activeOutages = [];
    
    const outage: ActiveOutage = {
      AffectedConsumers: [],
      DefaultIsolationPoints: [],
      ElementId: 123,
      FixedAt: new Date(),
      Id: 213,
      IsResolveConditionValidated: false,
      IsolatedAt: new Date(),
      OptimalIsolationPoints: [],
      RepairedAt: new Date(),
      ReportedAt: new Date(),
      State: OutageLifeCycleState.Created
    }

    this.activeOutages.push(outage);
    this.activeOutages.push(outage);

    this.getActiveOutages();
  }

  private getActiveOutages(): void {
    this.outageService.getAllActiveOutages().subscribe(
      outages => this.activeOutages = outages, 
      err => console.log(err)
    );
  }

  showMoreDetails(outage: ActiveOutage) : void{
     const dialogRef = this.dialog.open(ActiveOutageModalComponent, {
       data: outage
     });
  }

  public getOutageStateString(state: any) : string {
    return OutageLifeCycleState[state];
  }

}

