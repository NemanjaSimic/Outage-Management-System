export interface ActiveOutage {
    Id: number;
    ElementId: number;
    DateCreated: Date;
    AfectedConsumers: number[];
}

export interface ArchivedOutage extends ActiveOutage {
    DateArchived : Date;
}