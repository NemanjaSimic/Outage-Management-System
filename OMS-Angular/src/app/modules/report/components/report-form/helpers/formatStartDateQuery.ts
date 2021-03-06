import * as moment from 'moment';
import { DateType } from '@shared/models/report-options.model';

interface StartDateFormatMap {
  [dateType: string]: (date: moment.Moment) => string;
}

const startDateFormatMap: StartDateFormatMap = {
  [DateType.Yearly]: (date: moment.Moment) => `01/01/${date.format('YYYY')}`,
  [DateType.Monthly]: (date: moment.Moment) => `${date.format('MM')}/01/${date.format('YYYY')}`,
  [DateType.Daily]: (date: moment.Moment) => `${date.format('MM')}/${date.format('DD')}/${date.format('YYYY')}`,
}

export const formatStartDate = (date: moment.Moment, dateType: DateType): moment.Moment => moment(startDateFormatMap[dateType](date));
