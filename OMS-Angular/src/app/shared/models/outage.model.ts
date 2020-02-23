export interface Outage {
    Id: Number;
    ElementId: Number;
    State: OutageLifeCycleState;
    ReportedAt: Date;
    AffectedConsumers: Number[];
}

export interface ActiveOutage extends Outage {
    State: OutageLifeCycleState;
    DefaultIsolationPoints: Number[];
    OptimalIsolationPoints: Number[];
}

export interface ArchivedOutage extends Outage {
    ArchivedAt : Date;
    State: OutageLifeCycleState;
}

export enum OutageLifeCycleState {
    Created,
    Isolated,
    Archived
}