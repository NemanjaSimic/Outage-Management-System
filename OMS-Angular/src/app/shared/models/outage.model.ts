import { Consumer } from './consumer.model';

export interface ActiveOutage {
    Id: number;
    ElementId: number;
    ReportedAt: Date;
    AfectedConsumers: Consumer[];
}

export interface ArchivedOutage extends ActiveOutage {
    ArchivedAt : Date;
}