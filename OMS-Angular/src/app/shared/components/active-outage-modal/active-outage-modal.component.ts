import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { ActiveOutage, OutageLifeCycleState } from '@shared/models/outage.model';

@Component({
  selector: 'app-active-outage-modal',
  templateUrl: './active-outage-modal.component.html',
  styleUrls: ['./active-outage-modal.component.css']
})
export class ActiveOutageModalComponent implements OnInit {

  constructor(
    public dialogRef: MatDialogRef<ActiveOutage>,
    @Inject(MAT_DIALOG_DATA) public outage: ActiveOutage) { }

  ngOnInit() {
    console.log(this.outage);
  }

  onCloseClick(): void {
    this.dialogRef.close();
  }

  public getOutageStateString(state: any) : string {
    return OutageLifeCycleState[state];
  }

  public getOutagePointsString(points: any) : string[] {
    return points.map(point => `${point.Mrid} (0x${point.Id.toString(16).toUpperCase()})`);
  }

  public getConsumersString(consumers: any) : string[] {
    return consumers.map(consumer => 
      `${consumer.Mrid} (0x${consumer.Id.toString(16).toUpperCase()}) - ${consumer.LastName} ${consumer.FirstName}`);
  }
}
