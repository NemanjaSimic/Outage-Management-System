export interface ActiveOutage {
    Id: number;
    ElementId: number;
    ReportedAt: Date;
    AfectedConsumers: number[];
}

export interface ArchivedOutage extends ActiveOutage {
    ArchivedAt : Date;
}