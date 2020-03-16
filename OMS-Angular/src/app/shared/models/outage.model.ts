import { Consumer } from './consumer.model';
import { EquipmentViewModel } from './equipment.view-model';

export interface Outage {
    Id: Number;
    ElementId: Number;
    ReportedAt: Date;
    AffectedConsumers: Consumer[];
    IsolatedAt: Date;
    FixedAt: Date;
    RepairedAt: Date;
    State: OutageLifeCycleState;
    IsResolveConditionValidated: Boolean;
}

export interface ActiveOutage extends Outage {
    DefaultIsolationPoints: EquipmentViewModel[];
    OptimalIsolationPoints: EquipmentViewModel[];
}

export interface ArchivedOutage extends Outage {
    ArchivedAt : Date;
}

export enum OutageLifeCycleState {
    Created,
    Isolated,
    Repaired,
    Archived
}
