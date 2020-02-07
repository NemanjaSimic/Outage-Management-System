import { ActiveOutage, ArchivedOutage } from './outage.model';

export interface Consumer {
    Id: number;
    Mrid: string;
    FirstName: string;
    LastName: string;
    ActiveOutages: ActiveOutage[];
    ArchivedOutages: ArchivedOutage[];
}