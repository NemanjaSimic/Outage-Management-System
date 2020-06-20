import { Moment } from 'moment';

export interface ReportOptions {
  Type: ReportType,
  ElementId?: Number,
  StartDate?: Moment,
  EndDate?: Moment
}

export enum ReportType {
  Total = 0,
  SAIFI,
  SAIDI
}

export enum DateType {
  Yearly = 0,
  Monthly,
  Daily
}