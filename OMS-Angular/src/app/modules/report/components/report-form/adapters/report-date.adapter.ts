import { Injectable } from '@angular/core';
import { Moment } from 'moment';
import { DateFormatService } from '@services/report/date-format.service';
import { MomentDateAdapter } from '@angular/material-moment-adapter';

export const CUSTOM_DATE_FORMATS = {
  parse: {
    dateInput: { day: 'numeric', month: 'numeric', year: 'numeric' }
  },
  display: {
    dateInput: 'input',
    monthYearLabel: { year: 'numeric', month: 'short' },
    dateA11yLabel: { year: 'numeric', month: 'long', day: 'numeric' },
    monthYearA11yLabel: { year: 'numeric', month: 'long' },
  }
};

@Injectable()
export class ReportDateAdapter extends MomentDateAdapter {

  constructor(private dateFormatter: DateFormatService) {
    super("en-US");
  }

  public format(date: Moment, displayFormat: string): string {
    const format = this.dateFormatter.getFormat();
    const result = date.format(format);
    return result;
  }
}