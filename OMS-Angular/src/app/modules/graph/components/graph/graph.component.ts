import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { OmsGraph } from '@shared/models/oms-graph.model';

import { drawBackupEdge } from '@shared/utils/backup-edge';
import { addGraphTooltip, addEdgeTooltip, addAnalogMeasurementTooltip } from '@shared/utils/tooltip';
import { drawWarningOnNode, drawWarningOnLine } from '@shared/utils/warning';
import { drawCallWarning } from '@shared/utils/outage';
import { addOutageTooltip } from '@modules/graph/outage-lifecycle/tooltip';
import { modifyNodeDistance } from '@shared/utils/graph-distance';
import * as mapper from '@shared/utils/mapper';
import {
  drawMeasurements,
  GetUnitMeasurement,
  GetAlarmColorForMeasurement
} from '@shared/utils/measurement';

import { zoom } from '@shared/utils/zoom';
import { OutageService } from '@services/outage/outage.service';
import { CommandService } from '@services/command/command.service';
import { ScadaService } from '@services/notification/scada.service';
import { GraphService } from '@services/notification/graph.service';
import { OutageNotificationService } from '@services/notification/outage-notification.service';

import { IMeasurement } from '@shared/models/node.model';
import { ScadaData } from '@shared/models/scada-data.model';
import { SwitchCommand } from '@shared/models/switch-command.model';
import {
  ActiveOutage,
  ArchivedOutage,
  OutageLifeCycleState
} from '@shared/models/outage.model';

// cytoscape plugins
import dagre from 'cytoscape-dagre';
import cyConfig from './graph.config';
import popper from 'cytoscape-popper';
import * as cytoscape from 'cytoscape';
import * as legendData from './legend.json';

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
    backup_edges: []
  };

  private activeOutages: ActiveOutage[] = [];

  constructor(
    private graphService: GraphService,
    private scadaService: ScadaService,
    private outageNotificationService: OutageNotificationService,
    private commandService: CommandService,
    private outageService: OutageService,
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
    this.getActiveOutages();

    this.legendItems = legendData.items;
    this.drawGraph();
  }

  openMenu(event, message) : void {
    console.log(event.clientX);
    console.log(message);
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
      graph => this.onNotification(graph),
      error => console.log(error)
    );
  }

  public getActiveOutages(): void {
    this.outageService.getAllActiveOutages().subscribe(
      outages => {
        console.log(outages);
        this.activeOutages = outages;
        this.addOutageTooltips();
      },
      err => console.log(err)
    );
  }

  public startConnection(): void {
    this.connectionSubscription = this.graphService.startConnection().subscribe(
      (didConnect) => {
        if (didConnect) {
          console.log('Connected to graph service');

          this.updateSubscription = this.graphService.updateRecieved.subscribe(
            data => this.onNotification(data),
            err => console.log(err));

          this.outageSubscription = this.graphService.outageRecieved.subscribe(
            data => drawCallWarning(this.cy, data),
            err => console.log(err));

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
            (data: ScadaData) => this.onScadaNotification(data),
            err => console.log(err));
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
            (data: ActiveOutage) => this.onActiveOutageNotification(data),
            err => console.log(err));

          this.archivedOutageSubcription = this.outageNotificationService.archivedOutageUpdateRecieved.subscribe(
            (data: ArchivedOutage) => this.onArchivedOutageNotification(data),
            err => console.log(err));

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

    if (hasNodes && !this.zoomLevel)
      this.zoomLevel = this.cy.zoom();

    if (hasNodes && !this.panPosition)
      this.panPosition = this.cy.pan();

    this.cy.zoom(this.zoomLevel);
    this.cy.pan(this.panPosition);

    this.drawMeasurements();
    this.addTooltips();
    this.addOutageTooltips();
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
        node.sendSwitchCommand = (command) => this.onSwitchCommandHandler(command);
        if(node.data('type') != "analogMeasurement"){
          addGraphTooltip(this.cy, node);
        }
        if (node.data('dmsType') == "ACLINESEGMENT") {
          const connectedEdges = node.connectedEdges();
          if (connectedEdges.length)
            connectedEdges.map(acLineEdge => addEdgeTooltip(this.cy, node, acLineEdge));
        }
      });
    });
  }

  public drawMeasurements(): void {
    this.cy.ready(() => {
      this.cy.nodes().forEach(node => {
        let measurements: IMeasurement[] = node.data("measurements");
        if (measurements != undefined
          && !(measurements.length == 1
            && measurements[0].Type == "SWITCH_STATUS")
          && measurements.length != 0) {
          let measurementString = "";
          let nodePosition = 30;
          let counter = 1;
          let color = "#40E609";
          measurements.forEach(meas => {
            color = GetAlarmColorForMeasurement(meas.AlarmType);
            measurementString = meas.Value + " " + GetUnitMeasurement(meas.Type) + "\n";
            drawMeasurements(this.cy, node, measurementString, color, nodePosition * counter, meas.Id);
            counter++;
            let newNode = this.cy.$id(meas.Id);
            addAnalogMeasurementTooltip(this.cy, newNode, meas.AlarmType);
          });
        }
      })
    });
  }

  public drawWarnings(): void {
    this.cy.ready(() => {
      this.cy.edges().forEach(line => {
        drawWarningOnLine(this.cy, line);
      })
    });
  }

  public onSwitchCommandHandler = (command: SwitchCommand) => {
    this.commandService.sendSwitchCommand(command).subscribe(
      data => console.log(data),
      err => console.log(err)
    );
  }

  public onNotification(data: OmsGraph): void {
    this.ngZone.run(() => {
      this.graphData.nodes = data.Nodes.map(mapper.mapNode);
      this.graphData.edges = data.Relations.map(mapper.mapRelation);
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
              measurement.AlarmType = data[gid].Alarm;
              /*color = GetAlarmColorForMeasurement(data[gid].Alarm);
              measurementString = measurement.Value + " " + GetUnitMeasurement(measurement.Type) + "\n";*/
            }
          });
        });
      });
      this.drawGraph();
    });
  }

  public onActiveOutageNotification(outage: ActiveOutage): void {
    console.log(outage);
    this.activeOutages = this.activeOutages.filter(o => o.Id !== outage.Id);
    this.activeOutages.push(outage);
    this.drawGraph(); // da bi resetovao tooltip-ove, ako je velika mreza, optimizovacemo
  }

  public addOutageTooltips(): void {
    for (const activeOutage of this.activeOutages) {
      let outageElement;
      
      if (activeOutage.State == OutageLifeCycleState.Created) {
        if (activeOutage.DefaultIsolationPoints.length)
          outageElement = this.cy.nodes().filter(node => node.data('id') == activeOutage.DefaultIsolationPoints[0])[0];
      }

      // @TODO:
      // - proveriti sa Dimitrijem gde se iscrta kad je Repaired ?
      if (activeOutage.State == OutageLifeCycleState.Isolated 
        || activeOutage.State == OutageLifeCycleState.Repaired) 
      {
        outageElement = this.cy.nodes().filter(node => node.data('id') == activeOutage.ElementId)[0];
      }    

      if (outageElement) {
        const outageNodeId = drawWarningOnNode(this.cy, outageElement);
        const outageNode = this.cy.nodes().filter(node => node.data('id') == outageNodeId)[0];
        outageNode.sendIsolateOutageCommand = (id) => this.onIsolateOutageCommand(id);
        outageNode.sendRepairCrewCommand = (id) => this.onSendCrewOutageCommand(id);
        outageNode.sendValidateOutageCommand = (id) => this.onValidateOutageCommand(id);
        outageNode.sendResolveOutageCommand = (id) => this.onResolveOutageCommand(id);
        addOutageTooltip(this.cy, outageNode, activeOutage);
      }
    }
  }

  public onIsolateOutageCommand(id: Number): void {
    this.outageService.sendIsolateOutageCommand(id).subscribe(
      status => console.log(status),
      err => console.log(err)
    );
  }

  public onSendCrewOutageCommand(id: Number): void {
    this.outageService.sendOutageRepairCrew(id).subscribe(
      status => console.log(status),
      err => console.log(err)
    );
  }

  public onValidateOutageCommand(id: Number): void {
    this.outageService.sendValidateOutageCommand(id).subscribe(
      status => console.log(status),
      err => console.log(err)
    );
  }

  public onResolveOutageCommand(id: Number): void {
    this.outageService.sendResolveOutageCommand(id).subscribe(
      status => console.log(status),
      err => console.log(err)
    );

    this.activeOutages = this.activeOutages.filter(o => o.Id !== id);
    this.drawGraph();
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
