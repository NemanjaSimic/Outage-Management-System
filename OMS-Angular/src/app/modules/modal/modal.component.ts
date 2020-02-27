import { Component, Inject } from '@angular/core';
import { ActiveOutage, OutageLifeCycleState } from '@shared/models/outage.model';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  /*styleUrls: ['./modal.component.css']*/
})

/*@Injectable({ providedIn: 'root' })*/
export class ModalComponent{
  activeOutage : ActiveOutage;
  state : String;
  constructor(
    @Inject(MAT_DIALOG_DATA) private data: any
    ,private dialogRef: MatDialogRef<ModalComponent>) 
    {
      if(data)
      {
        this.activeOutage = data;
        this.state = OutageLifeCycleState[this.activeOutage.State];
      }
    }

  closeModal() {
    this.dialogRef.close();
  }

}
