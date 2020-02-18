import uuid from 'uuid';
import { AlarmType } from '@shared/models/scada-data.model';

export const GetUnitMeasurement = (type : string ) => {
    let retVal : string;
    switch(type){
        case "VOLTAGE":{
            retVal = "V";
            break;
        } 
        case "CURRENT":{
            retVal = "A";
            break;
        }
        case "POWER":{
            retVal = "W";
            break;
        }
        default:{
            retVal = "NaN";
            break;
        }
    }
    return retVal;
}

export const GetAlarmColorForMeasurement = (alarm : AlarmType) => {
    let retVal : string;
    switch(alarm){
        case AlarmType.NO_ALARM:{
            retVal = "#40E609"; //green
            break;
        }
        case AlarmType.LOW_ALARM:{
            retVal = "#f0f00c"; //yellow
            break;
        }
        case AlarmType.HIGH_ALARM:{
            retVal = "#f0670c"; //orange
            break;
        }
        case AlarmType.ABNORMAL_VALUE:{
            retVal = "#f0100c"; //red
            break; 
        }
        case AlarmType.REASONABILITY_FAILURE:{
            retVal = "#f0100c"; //red opet
            break;
        }
        default:{
            retVal = "#000000"; //crnilo
            break;
        }
    }
    return retVal;
}

export const drawMeasurements = (cy, node, measurementString, alarmColor, nodePosition) => {
    cy.add([
        {
        group: "nodes",
        data: {
            id: `${uuid()}`,
            type: 'analogMeasurement',
            content: measurementString,
            color : alarmColor
        },
        position: {
            x: node.position("x") - 30,
            y: node.position("y") - nodePosition
        }
        }
    ])
}