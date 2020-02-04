export interface ActiveOutage {
    Id: number;
    ElementId: number;
    DateCreated: Date;
    //TODO: prilikom preuzimanja podataka, voditi racuna da ce sa backend-a stici SCV string koji treba parsirati
    AfectedConsumers: number[];
}

export interface ArchivedOutage extends ActiveOutage {
    DateArchived : Date;
}