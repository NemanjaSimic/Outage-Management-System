import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { Subscription, Observable, fromEvent } from 'rxjs';
import { GraphService } from '@services/notification/graph.service';
import { OmsGraph } from '@shared/models/oms-graph.model';

import cyConfig from './graph.config';
import { addGraphTooltip } from '@shared/utils/tooltip';
import { drawWarning } from '@shared/utils/warning';

import * as cytoscape from 'cytoscape';
import * as mapper from '@shared/utils/mapper';
import * as graphMock from './graph-mock.json';

// cytoscape plugins
import dagre from 'cytoscape-dagre';
import popper from 'cytoscape-popper';
cytoscape.use(dagre);
cytoscape.use(popper);


@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.css']
})
export class GraphComponent implements OnInit, OnDestroy {
  public connectionSubscription: Subscription;
  public updateSubscription: Subscription;
  public zoomSubscription: Subscription;

  private cy: any;

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
    this.startConnection();

    // local testing
    this.graphData.nodes = graphMock.nodes;
    this.graphData.edges = graphMock.edges;

    this.drawGraph();

    // zoom on + and -
    this.zoomSubscription = fromEvent(document, 'keypress').subscribe(
      (e: KeyboardEvent) => {
        if (e.key == '+')
          this.cy.zoom(this.cy.zoom() + 0.1);
        else if (e.key == '-')
          this.cy.zoom(this.cy.zoom() - 0.1);
      });
  }

  ngOnDestroy() {
    if (this.connectionSubscription)
      this.connectionSubscription.unsubscribe();

    if (this.updateSubscription)
      this.updateSubscription.unsubscribe();

    if (this.zoomSubscription)
      this.zoomSubscription.unsubscribe();
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
      ...cyConfig,
      container: document.getElementById('graph'),
      elements: this.graphData
    });

    this.addTooltips();
    this.drawWarnings();
  };

  public addTooltips(): void {
    this.cy.ready(() => {
      this.cy.nodes().forEach(node => {
        addGraphTooltip(this.cy, node);
      });
    });
  }

  public drawWarnings(): void {
    this.cy.ready(() => {
      this.cy.edges().forEach(line => {
        drawWarning(this.cy, line);
      })
    });
  };

  public onNotification(data: OmsGraph): void {
    this.ngZone.run(() => {
      this.graphData.nodes = data.Nodes.map(mapper.mapNode);
      this.graphData.edges = data.Relations.map(mapper.mapRelation);
      this.drawGraph();
    });
  }

}
