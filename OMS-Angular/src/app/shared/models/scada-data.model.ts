import { NumberDictionary } from './dictionary.model';

export interface ScadaData {
    Data: NumberDictionary<AnalogModbusData>;
}

export interface AnalogModbusData {
    Value: number;
    Alarm: AlarmType;
}

export enum AlarmType {
    NO_ALARM = 0x01,
    REASONABILITY_FAILURE = 0x02,
    LOW_ALARM = 0x03,
    HIGH_ALARM = 0x04,
    ABNORMAL_VALUE = 0x05,
}