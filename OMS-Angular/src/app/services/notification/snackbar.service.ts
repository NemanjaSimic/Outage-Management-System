import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {

  private config: MatSnackBarConfig = {
    duration: 3000,
    horizontalPosition: 'right',
    panelClass: 'white-text'
  }

  constructor(private snackBar: MatSnackBar) { }

  public notify(message: string, action: string = 'OK'): void {
    this.snackBar.open(message, action, this.config);
  }
}
