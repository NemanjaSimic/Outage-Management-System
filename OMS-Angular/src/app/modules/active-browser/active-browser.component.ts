import { Component, OnInit } from '@angular/core';

import { ActiveOutage, OutageLifeCycleState } from '@shared/models/outage.model';
import { OutageService } from '@services/outage/outage.service';

import { MatDialog } from '@angular/material';
import { ModalComponent } from '@modules/modal/modal.component';

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
    this.GetActiveOutages();
  }

  private GetActiveOutages(): void {
    this.outageService.getAllActiveOutages().subscribe(
      outages => this.activeOutages = outages, 
      err => console.log(err)
    );

    console.log(this.activeOutages);
  }

  MoreDetails(outage: ActiveOutage) : void{
     const dialogRef = this.dialog.open(ModalComponent, {
       data: outage
     })
  }

  public getOutageStateString(state: any) : string {
    return OutageLifeCycleState[state];
  }

}

