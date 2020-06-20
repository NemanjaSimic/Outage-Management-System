import * as moment from 'moment';
import { DateType } from '@shared/models/report-options.model';

interface EndDateFormatMap {
  [dateType: string]: (date: moment.Moment) => string;
}

const endDateFormatMap: EndDateFormatMap = {
  [DateType.Yearly]: (date: moment.Moment) => `31/12/${date.format('YYYY')}`,
  [DateType.Monthly]: (date: moment.Moment) => `${moment(date).endOf('month').format('DD')}/${date.format('MM')}/${date.format('YYYY')}`,
  [DateType.Daily]: (date: moment.Moment) => `${date.format('DD')}/${date.format('MM')}/${date.format('YYYY')}`,
}

export const formatEndDate = (date: moment.Moment, dateType: DateType): moment.Moment => moment(endDateFormatMap[dateType](date));
