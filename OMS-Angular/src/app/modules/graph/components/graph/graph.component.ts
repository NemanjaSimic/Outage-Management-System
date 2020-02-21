import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { Subscription, Observable, fromEvent } from 'rxjs';
import { OmsGraph } from '@shared/models/oms-graph.model';

import cyConfig from './graph.config';
import { drawBackupEdge } from '@shared/utils/backup-edge';
import { addGraphTooltip, addOutageTooltip, addEdgeTooltip} from '@shared/utils/tooltip';
import { drawWarning } from '@shared/utils/warning';
import { drawCallWarning } from '@shared/utils/outage';
import { drawMeasurements, GetUnitMeasurement } from '@shared/utils/measurement';

import * as cytoscape from 'cytoscape';
import * as mapper from '@shared/utils/mapper';
import * as graphMock from './graph-mock.json';
import * as legendData from './legend.json';

// cytoscape plugins
import dagre from 'cytoscape-dagre';
import popper from 'cytoscape-popper';
import { SwitchCommand } from '@shared/models/switch-command.model';
import { zoom } from '@shared/utils/zoom';
import { ScadaData } from '@shared/models/scada-data.model';
import { IMeasurement } from '@shared/models/node.model';
import { modifyNodeDistance } from '@shared/utils/graph-distance';

import { GraphService } from '@services/notification/graph.service';
import { CommandService } from '@services/command/command.service';
import { ScadaService } from '@services/notification/scada.service';
import { OutageService } from '@services/outage/outage.service';
import { OutageNotificationService } from '@services/notification/outage-notification.service';

import { ActiveOutage, ArchivedOutage } from '@shared/models/outage.model';

cytoscape.use(dagre);
cytoscape.use(popper);


@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.css']
})
export class GraphComponent implements OnInit, OnDestroy {
  public panSubscription: Subscription;
  public zoomSubscription: Subscription;
  public scadaSubscription: Subscription;
  public updateSubscription: Subscription;
  public outageSubscription: Subscription;
  public topologySubscription: Subscription;
  public connectionSubscription: Subscription;
  public activeOutageSubcription: Subscription;
  public archivedOutageSubcription: Subscription;
  public scadaServiceConnectionSubscription: Subscription;
  public outageServiceConnectionSubscription: Subscription;

  private cy: any;
  public legendItems;
  public gidSearchQuery: string;

  public zoomLevel: number;
  public panPosition: object;

  private graphData: any = {
    nodes: [],
    edges: [],
    backup_edges: [],
    outages: []
  };

  constructor(
    private graphService: GraphService,
    private scadaService: ScadaService,
    private outageNotificationService: OutageNotificationService,
    private commandService: CommandService,
    private outageCommandService: OutageService,
    private ngZone: NgZone
  ) {
    this.connectionSubscription = Subscription.EMPTY;
    this.updateSubscription = Subscription.EMPTY;
    this.outageSubscription = Subscription.EMPTY;
  }

  ngOnInit() {
    // web api
    this.getTopology();
    this.startConnection();
    this.startScadaConnection();
    this.startOutageConnection();

    this.cy = cytoscape({});

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

    this.legendItems = legendData.items;
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

  public startOutageConnection(): void {
    this.outageServiceConnectionSubscription = this.outageNotificationService.startConnection().subscribe(
      (didConnect) => {
        if (didConnect) {
          console.log('Connected to Outage Notification service');

          this.activeOutageSubcription = this.outageNotificationService.activeOutageUpdateRecieved.subscribe(
            (data: ActiveOutage) => this.onActiveOutageNotification(data)
          );

          this.archivedOutageSubcription = this.outageNotificationService.archivedOutageUpdateRecieved.subscribe(
            (data: ArchivedOutage) => this.onArchivedOutageNotification(data)
          );
        }
        else {
          console.log('Could not connect to Outage Notification service');
        }
      },
      (err) => console.log(err)
    );
  }

  public drawGraph(): void {
    const hasNodes = this.graphData.nodes.length ? true : false;

    if (hasNodes && this.zoomLevel && this.panPosition) {
      this.zoomLevel = this.cy.zoom();
      this.panPosition = this.cy.pan();
    }

    this.cy = cytoscape({
      ...cyConfig,
      container: document.getElementById('graph'),
      elements: this.graphData
    }); 

    if(hasNodes && !this.zoomLevel)
      this.zoomLevel = this.cy.zoom();
    
    if(hasNodes && !this.panPosition)
      this.panPosition = this.cy.pan();
      
    this.cy.zoom(this.zoomLevel);
    this.cy.pan(this.panPosition);

    this.drawMeasurements();
    this.addTooltips();
    modifyNodeDistance(this.cy.nodes().filter(x => x.data('dmsType') == "ENERGYCONSUMER"));
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
        if (node.data("type") == 'warning') {
          var outage;
          this.graphData.outages.forEach(out => {
            var outageId = out["data"]["elementId"];
            if (node.data("targetId") == outageId) {
              outage = out;
            }
          });

          addOutageTooltip(this.cy, node, outage);
        }
	    	else if(node.data("type") == 'analogMeasurement')
        {
          //ako se bude dodavao tooltip sa tipom alarma
        }
        else {

          addGraphTooltip(this.cy, node);
          if (node.data('dmsType') == "ACLINESEGMENT") {
            const connectedEdges = node.connectedEdges();
            if (connectedEdges.length)
              connectedEdges.map(acLineEdge => addEdgeTooltip(this.cy, node, acLineEdge));
          }
        };
      });
    });
  }

  public drawMeasurements(): void {
    this.cy.ready(() => {
      this.cy.nodes().forEach(node => {
        let measurements : IMeasurement[] = node.data("measurements");
        if(measurements != undefined 
            && !(measurements.length == 1 
            && measurements[0].Type == "SWITCH_STATUS") 
            && measurements.length != 0)
        {
          let measurementString = "";
          measurements.forEach(meas => {
            measurementString += meas.Value + " " + GetUnitMeasurement(meas.Type) + "\n";
          });
          drawMeasurements(this.cy, node, measurementString);
        }
      })
    });
  }

  public drawWarnings(): void {
    this.cy.ready(() => {
      this.cy.edges().forEach(line => {
        drawWarning(this.cy, line);
      })
    });
  }

  public onCommandHandler = (command: SwitchCommand) => {
    this.commandService.sendSwitchCommand(command).subscribe(
      data => console.log(data),
      err => console.log(err)
    );
  }

  public onNotification(data: OmsGraph): void {
    this.ngZone.run(() => {
      this.graphData.nodes = data.Nodes.map(mapper.mapNode);
      this.graphData.edges = data.Relations.map(mapper.mapRelation);
      console.log(this.graphData);
      this.drawGraph();
    });
  }

  public onScadaNotification(data: ScadaData): void {
    this.ngZone.run(() => {
      let gids = Object.keys(data);
      gids.forEach(gid => {
        this.graphData.nodes.forEach(node => {
          let msms = node.data["measurements"];
          msms.forEach(measurement => {
            if (measurement.Id == gid) {
              measurement.Value = data[gid].Value;
            }
          });
        });
      });
      this.drawGraph();
    });
    console.log(data);
  }

  public onActiveOutageNotification(data: ActiveOutage): void {
    console.log('onActiveOutageNotification');
    drawCallWarning(this.cy, data.ElementId);
    this.ngZone.run(() => {
      this.cy.nodes().forEach(node => {
        if(node.data("id") == data.ElementId)
        {
          drawWarning(this.cy, node);
          addOutageTooltip(this.cy, node, data);
        }
        });
    });
  }

  public onArchivedOutageNotification(data: ArchivedOutage): void {
    console.log('onArchivedOutageNotification');
    console.log(data);
  }

  public onSearch(): void {
    this.cy.ready(() => {
      zoom(this.cy, this.gidSearchQuery);
    })
  }
}
