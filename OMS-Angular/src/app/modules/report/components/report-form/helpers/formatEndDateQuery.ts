import * as moment from 'moment';
import { DateType } from '@shared/models/report-options.model';

interface EndDateFormatMap {
  [dateType: string]: (date: moment.Moment) => string;
}

const endDateFormatMap: EndDateFormatMap = {
  [DateType.Yearly]: (date: moment.Moment) => `12/31/${date.format('YYYY')}`,
  [DateType.Monthly]: (date: moment.Moment) => `${date.format('MM')}/${moment(date).endOf('month').format('DD')}/${date.format('YYYY')}`,
  [DateType.Daily]: (date: moment.Moment) => `${date.format('MM')}/${date.format('DD')}/${date.format('YYYY')}`,
}

export const formatEndDate = (date: moment.Moment, dateType: DateType): moment.Moment => moment(endDateFormatMap[dateType](date));
