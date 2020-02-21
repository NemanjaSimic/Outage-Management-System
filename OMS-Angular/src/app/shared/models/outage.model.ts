export interface ActiveOutage {
    Id: number;
    ElementId: number;
    State: OutageLifeCycleState;
    ReportedAt: Date;
    AfectedConsumers: number[];
}

export interface ArchivedOutage extends ActiveOutage {
    ArchivedAt : Date;
    State: OutageLifeCycleState.Isolated;
}

export enum OutageLifeCycleState {
    Created,
    Isolated,
    Archived
}