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
  Value: Number, 
  Type: string
}
