import { Component, OnInit, OnDestroy } from '@angular/core';

import { ArchivedOutage } from '@shared/models/outage.model';
import { OutageService } from '@services/outage/outage.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-archived-browser',
  templateUrl: './archived-browser.component.html',
  styleUrls: ['./archived-browser.component.css']
})

export class ArchivedBrowserComponent implements OnInit, OnDestroy {
  private archivedOutagesSubscription: Subscription;

  private archivedOutages: ArchivedOutage[] = [];
  private columns: string[] = ["id", "elementId", "reportedAt", "archivedAt"];

  constructor(private outageService: OutageService) { }

  ngOnInit() {
    this.archivedOutagesSubscription = this.outageService.getAllArchivedOutages().subscribe(
      outages => this.archivedOutages = outages,
      err => console.log(err)
    );
  }

  ngOnDestroy() {
    if(!this.archivedOutagesSubscription)
      this.archivedOutagesSubscription.unsubscribe();
  }

}

