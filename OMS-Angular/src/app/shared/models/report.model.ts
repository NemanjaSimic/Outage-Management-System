import { StringDictionary } from './dictionary.model';

export interface Report {
  Data: StringDictionary<Number>,
  Type: String
}