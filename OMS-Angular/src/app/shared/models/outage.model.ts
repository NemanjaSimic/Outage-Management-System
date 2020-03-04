import { Consumer } from './consumer.model';

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
    DefaultIsolationPoints: Number[];
    OptimalIsolationPoints: Number[];
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
