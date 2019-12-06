import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { GraphService } from '@services/notification/graph.service';
import { Subscription } from 'rxjs';
import { OmsGraph } from '@shared/models/oms-graph.model';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.css']
})
export class GraphComponent implements OnInit, OnDestroy {
  private connectionSubscription: Subscription;
  private updateSubscription: Subscription;

  private graphData = {
    nodes: [
      { data: { id: "1", name: "test", faveColor: "blue"} },
      { data: { id: "2", name: "test", faveColor: "blue"} }
    ],
    edges: [
      { data: { source: "1", target: "2" }}
    ]
  }

  constructor(
    private graphService: GraphService,
    private ngZone: NgZone
  ) { }

  ngOnInit() {
    this.connectionSubscription = this.graphService.startConnection().subscribe(
      (didConnect) => {
        if (didConnect)
          console.log('Connected to graph service');
        else
          console.log('Could not connect to graph service');
      },
      (err) => console.log(err)
    );

    this.updateSubscription = this.graphService.updateRecieved.subscribe(
      data => this.onNotification(data));
  }

  ngOnDestroy() {
    if (this.connectionSubscription)
      this.connectionSubscription.unsubscribe();

    if (this.updateSubscription)
      this.updateSubscription.unsubscribe();
  }

  public onNotification(data: OmsGraph) {
    this.ngZone.run(() => {
      console.log(this.graphData.nodes);

      this.graphData.nodes = data.Nodes.map(node => {
        return {
          data: {
            id: node.Id,
            label: node.Name
          }
        }
      });

      this.graphData.edges = data.Relations.map(relation => {
        return {
          data: {
            source: relation.SourceNodeId,
            target: relation.TargetNodeId
          }
        }
      });

      console.log(this.graphData.nodes);
    });
  }

}
