import { Injectable } from '@angular/core';
import {  DateType } from '@shared/models/report-options.model';

export interface DateFormatMap {
  [dateType: string]: string;
}

@Injectable({
  providedIn: 'root'
})
export class DateFormatService {
  private dateFormatMap: DateFormatMap = {
    [DateType.Yearly]: "YYYY",
    [DateType.Monthly]: "MM/YYYY",
    [DateType.Daily]: "DD/MM/YYYY",
  }

  private format;

  constructor() {
    this.format = "";
  }

  public getFormat(): string {
    return this.format;
  }

  public setFormat(dateType: DateType) {
    this.format = this.dateFormatMap[dateType];
  }


}
