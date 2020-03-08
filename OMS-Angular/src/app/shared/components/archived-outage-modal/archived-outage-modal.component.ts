import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { ArchivedOutage, OutageLifeCycleState } from '@shared/models/outage.model';

@Component({
  selector: 'app-archived-outage-modal',
  templateUrl: './archived-outage-modal.component.html',
  styleUrls: ['./archived-outage-modal.component.css']
})
export class ArchivedOutageModalComponent implements OnInit {

  constructor(
    public dialogRef: MatDialogRef<ArchivedOutage>,
    @Inject(MAT_DIALOG_DATA) public outage: ArchivedOutage) { }

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
}
