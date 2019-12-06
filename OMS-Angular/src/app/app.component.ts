import { Component, OnInit, NgZone } from '@angular/core';
import { GraphService } from '@services/notification/graph.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  constructor(
    private graphService: GraphService,
    private ngZone: NgZone
  ) { }

  ngOnInit() {
    console.log('AppComponent inited.');
    this.graphService.startConnection().subscribe(
      data => console.log(data),
      err => console.log(err)
    );
    
    this.graphService.updateRecieved.subscribe(data => this.onNotification(data));
  }

  public onNotification(data) {
    this.ngZone.run(() => {
      console.log('Recieved data on AppComponent!');
      console.log(data);
    });
  }

}
