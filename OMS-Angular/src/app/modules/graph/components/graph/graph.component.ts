import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { GraphService } from '@services/notification/graph.service';
import { Subscription } from 'rxjs';
import { OmsGraph } from '@shared/models/oms-graph.model';

import * as cytoscape from 'cytoscape';
import dagre from 'cytoscape-dagre';
import { style } from './graph.style';
cytoscape.use(dagre);

import * as graphMock from './graph-mock.json';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.css']
})
export class GraphComponent implements OnInit, OnDestroy {
  public connectionSubscription: Subscription;
  public updateSubscription: Subscription;
  
  private cy: any;
  private cyConfig: Object = {
    layout: { name: 'dagre', rankDir: 'TB' },
    autoungrabify: true,
    style: style,
    wheelSensitivity: 0.1
  };

  private graphData: any = {
    nodes: [],
    edges: []
  };

  constructor(
    private graphService: GraphService,
    private ngZone: NgZone
  ) { 
    this.connectionSubscription = Subscription.EMPTY;
    this.updateSubscription = Subscription.EMPTY;
  }

  ngOnInit() {
    // web api
    // this.startConnection();
    
    // local testing
    this.graphData.nodes = graphMock.nodes;
    this.graphData.edges = graphMock.edges;
    this.drawGraph();
  }

  ngOnDestroy() {
    if (this.connectionSubscription)
      this.connectionSubscription.unsubscribe();

    if (this.updateSubscription)
      this.updateSubscription.unsubscribe();
  }

  public startConnection(): void {
    this.connectionSubscription = this.graphService.startConnection().subscribe(
      (didConnect) => {
        if (didConnect) {
          console.log('Connected to graph service');

          this.updateSubscription = this.graphService.updateRecieved.subscribe(
            data => this.onNotification(data));

          this.drawGraph();
        }
        else {
          console.log('Could not connect to graph service');
        }
      },
      (err) => console.log(err)
    );
  }

  public drawGraph(): void {
    this.cy = cytoscape({
      container: document.getElementById('graph'),
      elements: this.graphData,
      ...this.cyConfig
    })
  };

  public onNotification(data: OmsGraph): void {
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

}
