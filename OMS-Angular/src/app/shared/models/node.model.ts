import { AlarmType } from './scada-data.model';

export interface Node {
  Id: string;
  Name: string;
  Mrid: string;
  Description: string;
  DMSType: string;
  Measurements: IMeasurement[]
  NominalVoltage: string;
  IsRemote: Boolean;
  IsActive: Boolean;
}

export interface IMeasurement {
  Id: string;
  Type: string;
  Value: Number;
  AlarmType: AlarmType; 
}
