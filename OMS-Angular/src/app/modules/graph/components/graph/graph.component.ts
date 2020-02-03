import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { Subscription, Observable, fromEvent } from 'rxjs';
import { GraphService } from '@services/notification/graph.service';
import { OmsGraph } from '@shared/models/oms-graph.model';

import cyConfig from './graph.config';
import { drawBackupEdge } from '@shared/utils/backup-edge';
import { addGraphTooltip, addOutageTooltip } from '@shared/utils/tooltip';
import { drawWarning } from '@shared/utils/warning';
import { drawCallWarning } from '@shared/utils/outage';

import * as cytoscape from 'cytoscape';
import * as mapper from '@shared/utils/mapper';
import * as graphMock from './graph-mock.json';

// cytoscape plugins
import dagre from 'cytoscape-dagre';
import popper from 'cytoscape-popper';
import { CommandService } from '@services/command/command.service';
import { SwitchCommandType, SwitchCommand } from '@shared/models/switch-command.model';
import { zoom } from '@shared/utils/zoom';
import { ScadaService } from '@services/notification/scada.service';
import { ScadaData } from '@shared/models/scada-data.model';
cytoscape.use(dagre);
cytoscape.use(popper);


@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.css']
})
export class GraphComponent implements OnInit, OnDestroy {
  public connectionSubscription: Subscription;
  public topologySubscription: Subscription;
  public updateSubscription: Subscription;
  public outageSubscription: Subscription;

  public scadaServiceConnectionSubscription: Subscription;
  public scadaSubscription: Subscription;
  
  public zoomSubscription: Subscription;
  public panSubscription: Subscription;

  public gidSearchQuery: string;
  public didLoadGraph: boolean;
  private cy: any;

  private graphData: any = {
    nodes: [],
    edges: [],
    backup_edges: [],
    outages: []
  };

  constructor(
    private graphService: GraphService,
    private scadaService: ScadaService,
    private commandService: CommandService,
    private ngZone: NgZone
  ) {
    this.connectionSubscription = Subscription.EMPTY;
    this.updateSubscription = Subscription.EMPTY;
    this.outageSubscription = Subscription.EMPTY;
  }

  ngOnInit() {
    // testing splash screen look, will change logic after we connect to the api
    this.didLoadGraph = true;

    // web api
    this.getTopology();
    this.startConnection();
    this.startScadaConnection();

    // local testing
    //this.graphData.nodes = graphMock.nodes;
    //this.graphData.edges = graphMock.edges;
    //this.graphData.backup_edges = graphMock.backup_edges;
    //this.graphData.outages = graphMock.outages;

    //this.drawGraph(); // initial test

    // zoom on + and -
    this.zoomSubscription = fromEvent(document, 'keypress').subscribe(
      (e: KeyboardEvent) => {
        if (e.key == '+')
          this.cy.zoom(this.cy.zoom() + 0.1);
        else if (e.key == '-')
          this.cy.zoom(this.cy.zoom() - 0.1);
      });

    this.panSubscription = fromEvent(document, 'keydown').subscribe(
      (e: KeyboardEvent) => {
        if (e.key == 'ArrowLeft')
          this.cy.panBy({
            x: 50,
            y: 0
          });
        else if (e.key == 'ArrowRight')
          this.cy.panBy({
            x: -50,
            y: 0
          });
        else if (e.key == 'ArrowUp')
          this.cy.panBy({
            x: 0,
            y: 50
          });
        else if (e.key == 'ArrowDown')
          this.cy.panBy({
            x: 0,
            y: -50
          });
      }
    )
  }

  ngOnDestroy() {
    if (this.connectionSubscription)
      this.connectionSubscription.unsubscribe();

    if (this.topologySubscription)
      this.topologySubscription.unsubscribe();

    if (this.updateSubscription)
      this.updateSubscription.unsubscribe();

    if (this.scadaSubscription)
      this.scadaSubscription.unsubscribe();

    if (this.zoomSubscription)
      this.zoomSubscription.unsubscribe();

    if (this.outageSubscription)
      this.outageSubscription.unsubscribe();
  }

  public getTopology(): void {
    this.topologySubscription = this.graphService.getTopology().subscribe(
      graph => {
        console.log(graph);
        this.onNotification(graph);
      },
      error => console.log(error)
    );
  }

  public startConnection(): void {
    this.connectionSubscription = this.graphService.startConnection().subscribe(
      (didConnect) => {
        if (didConnect) {
          console.log('Connected to graph service');

          this.updateSubscription = this.graphService.updateRecieved.subscribe(
            data => this.onNotification(data));

          this.outageSubscription = this.graphService.outageRecieved.subscribe(
            data => drawCallWarning(this.cy, data));

          this.drawGraph();
        }
        else {
          console.log('Could not connect to graph service');
        }
      },
      (err) => console.log(err)
    );
  }

  public startScadaConnection(): void {
    this.scadaServiceConnectionSubscription = this.scadaService.startConnection().subscribe(
      (didConnect) => {
        if (didConnect) {
          console.log('Connected to scada service');

          this.scadaSubscription = this.scadaService.updateRecieved.subscribe(
            (data: ScadaData) => this.onScadaNotification(data));
        }
        else {
          console.log('Could not connect to scada service');
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

    //this.drawBackupEdges();
    this.drawWarnings();
    this.addTooltips();
  };

  public drawBackupEdges(): void {
    this.cy.ready(() => {
      this.graphData.backup_edges.forEach(line => {
        drawBackupEdge(this.cy, line);
      });
    });
  }

  public addTooltips(): void {
    this.cy.ready(() => {
      this.cy.nodes().forEach(node => {
        node.sendSwitchCommand = (command) => this.onCommandHandler(command);
        if(node.data("type") == 'warning')
        {
          //Za sad ovako jer su hardkodovani i Outage simboli, ovako je testirano samo
          var outage;
          this.graphData.outages.forEach(o => {
             var outageId = o["data"]["elementId"];
              if(node.data("targetId") == outageId)
              {
                outage = o;
              }
           });
           addOutageTooltip(this.cy, node, outage);
        }else
        {
          addGraphTooltip(this.cy, node);
        }
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

  public onCommandHandler = (command: SwitchCommand) => {
    this.commandService.sendSwitchCommand(command).subscribe(
      data => console.log(data),
      err => console.log(err)
    );
  }

  public onNotification(data: OmsGraph): void {
    this.ngZone.run(() => {
      console.log(data);
      this.graphData.nodes = data.Nodes.map(mapper.mapNode);
      this.graphData.edges = data.Relations.map(mapper.mapRelation);
      this.drawGraph();
    });
  }

  public onScadaNotification(data: ScadaData): void {
    console.log(data);
  }

  public onSearch() : void {
    this.cy.ready(() => {
      zoom(this.cy, this.gidSearchQuery);
    })
  }

}
