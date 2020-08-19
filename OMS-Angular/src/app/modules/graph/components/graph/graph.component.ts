import { Component, OnInit, NgZone, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { OmsGraph } from '@shared/models/oms-graph.model';

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
import { ScadaCoreService } from '@services/notification/core/scada.service.core';
import { GraphService } from '@services/notification/graph.service';
import { GraphCoreService } from '@services/notification/core/graph.service.core';
import { OutageNotificationService } from '@services/notification/outage-notification.service';
import { OutageNotificationCoreService } from '@services/notification/core/outage-notification.service.core';

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
import { SnackbarService } from '@services/notification/snackbar.service';

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
    // private graphService: GraphService,
    private graphCoreService: GraphCoreService,
    // private scadaService: ScadaService,
    private scadaCoreService: ScadaCoreService,
    // private outageNotificationService: OutageNotificationService,
    private outageNotificationCoreService: OutageNotificationCoreService,
    private commandService: CommandService,
    private outageService: OutageService,
    private snackBar: SnackbarService,
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
    this.topologySubscription = this.graphCoreService.getTopology().subscribe(
    // this.topologySubscription = this.graphService.getTopology().subscribe(
      graph => {
        console.log(graph);
        this.onNotification(graph)
      },
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
    this.graphCoreService.startConnection();
    this.updateSubscription =  this.graphCoreService.updateRecieved.subscribe(
      data => this.onNotification(data),
      err => console.log(err));
    
    this.outageSubscription = this.graphCoreService.outageRecieved.subscribe(
      data => drawCallWarning(this.cy, data),
      err => console.log(err));

    this.drawGraph();

    /// OBSOLETE (.NET SignalR)
    // this.connectionSubscription = this.graphService.startConnection().subscribe(
    //   (didConnect) => {
    //     if (didConnect) {
    //       console.log('Connected to graph service');

    //       this.updateSubscription = this.graphService.updateRecieved.subscribe(
    //         data => this.onNotification(data),
    //         err => console.log(err));

    //       this.outageSubscription = this.graphService.outageRecieved.subscribe(
    //         data => drawCallWarning(this.cy, data),
    //         err => console.log(err));

    //       this.drawGraph();
    //     }
    //     else {
    //       console.log('Could not connect to graph service');
    //     }
    //   },
    //   (err) => console.log(err)
    // );
  }

  public startScadaConnection(): void {
    this.scadaCoreService.startConnection();
    this.scadaSubscription = this.scadaCoreService.updateRecieved.subscribe(
      (data: ScadaData) => this.onScadaNotification(data),
      err => console.log(err));

    /// OBSOLETE (.NET SignalR)
    // this.scadaServiceConnectionSubscription = this.scadaService.startConnection().subscribe(
    //   (didConnect) => {
    //     if (didConnect) {
    //       console.log('Connected to scada service');

    //       this.scadaSubscription = this.scadaService.updateRecieved.subscribe(
    //         (data: ScadaData) => this.onScadaNotification(data),
    //         err => console.log(err));
    //     }
    //     else {
    //       console.log('Could not connect to scada service');
    //     }
    //   },
    //   (err) => console.log(err)
    // );
  }

  public startOutageConnection(): void {
    this.outageNotificationCoreService.startConnection();
    this.activeOutageSubcription = this.outageNotificationCoreService.activeOutageUpdateRecieved.subscribe(
      (data: ActiveOutage) => this.onActiveOutageNotification(data),
      err => console.log(err));

    this.archivedOutageSubcription = this.outageNotificationCoreService.archivedOutageUpdateRecieved.subscribe(
      (data: ArchivedOutage) => this.onArchivedOutageNotification(data),
      err => console.log(err));
            
    /// OBSOLETE (.NET SignalR)
    // this.outageServiceConnectionSubscription = this.outageNotificationService.startConnection().subscribe(
    //   (didConnect) => {
    //     if (didConnect) {
    //       console.log('Connected to Outage Notification service');

    //       this.activeOutageSubcription = this.outageNotificationService.activeOutageUpdateRecieved.subscribe(
    //         (data: ActiveOutage) => this.onActiveOutageNotification(data),
    //         err => console.log(err));

    //       this.archivedOutageSubcription = this.outageNotificationService.archivedOutageUpdateRecieved.subscribe(
    //         (data: ArchivedOutage) => this.onArchivedOutageNotification(data),
    //         err => console.log(err));

    //     }
    //     else {
    //       console.log('Could not connect to Outage Notification service');
    //     }
    //   },
    //   (err) => console.log(err)
    // );
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
    console.log("onNotification => omsGraph data:");
    console.log(data);

    this.ngZone.run(() => {
      this.graphData.nodes = data.Nodes.map(mapper.mapNode);
      this.graphData.edges = data.Relations.map(mapper.mapRelation);
      this.drawGraph();
    });
  }

  public onScadaNotification(data: ScadaData): void {
    console.log("onScadaNotification => scada data:");
    console.log(data);

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
    console.log("onActiveOutageNotification => ActiveOutage data:");
    console.log(outage);

    let message;
    if (outage.State == OutageLifeCycleState.Removed)
    {
      message = `Outage with gid ${outage.Id} has been removed.`;
    }
    else if(outage.State == OutageLifeCycleState.Isolated)
    {
      message = `Outage with ID ${outage.Id} has been successfully isolated.`;
    }
    else if(outage.State == OutageLifeCycleState.Repaired)
    {
      message = `Crew has repaired outage with ID ${outage.Id} successfully.`;
    }
    else if(outage.State == OutageLifeCycleState.Created)
    {
      message = `Outage with ID ${outage.Id} is successfully created.`;
    }
    else if(outage.State == OutageLifeCycleState.Archived)
    {
      message = `Outage with ID ${outage.Id} is successfully archived.`;
    }
    
    if (message)
    {
        this.snackBar.notify(message);
    }
    this.activeOutages = this.activeOutages.filter(o => o.Id !== outage.Id);
    this.activeOutages.push(outage);
    this.drawGraph(); // da bi resetovao tooltip-ove, ako je velika mreza, optimizovacemo
  }

  public addOutageTooltips(): void {
    for (const activeOutage of this.activeOutages) {
      let outageElement;
      
      if (activeOutage.State == OutageLifeCycleState.Created) {
        if (activeOutage.DefaultIsolationPoints.length)
          outageElement = this.cy.nodes().filter(node => node.data('id') == activeOutage.DefaultIsolationPoints[0].Id)[0];
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
        outageNode.sendSendLocationIsolationCrew = (id) => this.onSendLocationIsolationCrewCommand(id);
        addOutageTooltip(this.cy, outageNode, activeOutage, outageElement);
      }
    }
  }
  public onSendLocationIsolationCrewCommand(id: Number):void{
    this.outageService.sendLocationIsolationCrewCommand(id).subscribe(
      status =>
      {
        console.log("Status of send location and isolation crew is: ");
        console.log(status);
        // this.snackBar.notify(`Crew has isolated outage with ID ${id} successfully.`);
      },
      err => console.log(err)
    );
  }
  public onIsolateOutageCommand(id: Number): void {
    this.outageService.sendIsolateOutageCommand(id).subscribe(
      status => console.log(status),//this.snackBar.notify(`Outage with ID ${id} has been successfully isolated.`),
      err => console.log(err)
    );
  }

  public onSendCrewOutageCommand(id: Number): void {
    this.outageService.sendOutageRepairCrew(id).subscribe(
      status => console.log(status),//this.snackBar.notify(`Crew has repaired outage with ID ${id} successfully.`),
      err => console.log(err)
    );
  }

  public onValidateOutageCommand(id: Number): void {
    this.outageService.sendValidateOutageCommand(id).subscribe(
      status => console.log(status),//this.snackBar.notify(`Outage with ID ${id} has been successfully validated.`),
      err => console.log(err)
    );
  }

  public onResolveOutageCommand(id: Number): void {
    this.outageService.sendResolveOutageCommand(id).subscribe(
      status => console.log(status),//this.snackBar.notify(`Outage with ID ${id} has been successfully resolved.`),
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
