import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { GraphService } from '@services/notification/graph.service';
import { Subscription } from 'rxjs';
import { OmsGraph } from '@shared/models/oms-graph.model';

import * as cytoscape from 'cytoscape';
import dagre from 'cytoscape-dagre';
import { style } from './graph.style';
cytoscape.use(dagre);


@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.css']
})
export class GraphComponent implements OnInit, OnDestroy {
  private connectionSubscription: Subscription;
  private updateSubscription: Subscription;
  private cy: any;

  private graphData: any = {
    nodes: [],
    edges: []
  };

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

    this.drawGraph();
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
            label: node.Name,
            state: node.IsActive ? "active" : "inactive"
          }
        }
      });

      this.graphData.edges = data.Relations.map(relation => {
        return {
          data: {
            source: relation.SourceNodeId,
            target: relation.TargetNodeId,
            color: relation.IsActive ? "blue" : "red"
          }
        }
      });

      console.log(this.graphData.nodes);
      this.drawGraph();
    });
  }

  private drawGraph(): void {
    this.cy = cytoscape({
      container: document.getElementById('graph'),
      layout: { name: 'dagre', rankDir: 'TB' },
      autoungrabify: true,
      elements: this.graphData,
      style: style
    })
  };

}
