import { ReportOptions, ReportType } from '@shared/models/report-options.model';

export const buildQuery = (options: ReportOptions) => {
  let query = '';

  if(options.Type)
    query += `type=${options.Type}&`;
  else
    query += `type=${ReportType.Total}&`;
  
  if(options.ElementId)
    query += `elementId=${options.ElementId}&`;
  
  if(options.StartDate)
    query += `startDate=${options.StartDate.format('MM-DD-YYYY')}&`;
  
  if(options.EndDate)
    query += `endDate=${options.EndDate.format('MM-DD-YYYY')}`;

  return query;
}