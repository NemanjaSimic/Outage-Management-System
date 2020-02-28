export interface Outage {
    Id: Number;
    ElementId: Number;
    State: OutageLifeCycleState;
    ReportedAt: Date;
    AffectedConsumers: Number[];
    IsolatedAt: Date;
    FixedAt: Date;
    RepairedAt: Date;
}

export interface ActiveOutage extends Outage {
    IsValidated: Boolean;
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
    Repaired,
    Archived
}
