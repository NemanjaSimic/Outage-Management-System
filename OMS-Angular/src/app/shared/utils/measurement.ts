import uuid from 'uuid';

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

export const drawMeasurements = (cy, node, measurementString) => {
    cy.add([
        {
        group: "nodes",
        data: {
            id: `${uuid()}`,
            type: 'analogMeasurement',
            content: measurementString
        },
        position: {
            x: node.position("x") - 20,
            y: node.position("y") - 25
        }
        }
    ])
}