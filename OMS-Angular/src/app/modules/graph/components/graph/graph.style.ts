import * as cytoscape from 'cytoscape';

export const ACTIVE_NODE_COLOR = 'green'; 
export const ACTIVE_EDGE_COLOR = 'green'; 
export const INACTIVE_NODE_COLOR = 'blue'; 
export const INACTIVE_EDGE_COLOR = 'blue';

export const style = cytoscape.stylesheet()
    .selector('node[state = "active"]')
    .style({
        'background-color': ACTIVE_NODE_COLOR
    })
    .selector('node[state = "inactive"]')
    .style({
        'background-color': INACTIVE_NODE_COLOR
    })
    .selector('node[dmsType = "ENERGYCONSUMER"][state = "active"]')
    .style({
        'background-fit': 'cover',
        'background-opacity': '0',
        'background-image': 'assets/img/consumer.png',
        'height': '45px',
        'width': '45px',
    })
    .selector('node[dmsType = "ENERGYCONSUMER"][state = "inactive"]')
    .style({
        'background-fit': 'cover',
        'background-opacity': '0',
        'background-image': 'assets/img/plug.png',
        'height': '45px',
        'width': '45px',
    })
    .selector('node[type = "warning"]')
    .style({
        'shape': 'rectangle',
        'background-color': '#2b2935',
        'background-fit': 'cover',
        'background-image': 'assets/img/warning.png',
        'height': '20px',
        'width': '20px'
    })
    .selector('node[type = "analogMeasurement"]')
    .style({
        'shape': 'rectangle',
        'background-color': '#2b2935',
        'background-opacity': '0',
        'border' : '3px',
        'border-color' : "#40E609",
        'color' : 'data(color)',
        'background-fit': 'contain',
        'content': 'data(content)',
        'align-content' : 'center',
        'height': '1px',
        'width': '1px',
    })
    .selector('node[type = "outage-call"]')
    .style({
        'shape': 'rectangle',
        'background-color': '#2b2935',
        'background-fit': 'cover',
        'background-image': 'assets/img/outage-call.png',
        'height': '20px',
        'width': '20px'
    })
    .selector('node[dmsType="ENERGYSOURCE"]')
    .style({
        'background-fit': 'cover',
        'background-image': 'assets/img/energy2.png',
        'background-opacity': '0',
        'height': '100px',
        'width': '100px'
    })
    .selector('node[dmsType="POWERTRANSFORMER"]')
    .style({
        'background-fit': 'cover',
        'background-image': 'assets/img/power-transformer.png',
        'background-opacity': '0',
        'height': '100px',
        'width': '100px'
    })
    .selector('node[dmsType="FUSE"]')
    .style({
        'background-fit': 'cover',
        'background-image': 'assets/img/fuse.png',
        'background-opacity': '0',
        'height': '35px',
        'width': '35px'
    })
    .selector('node[dmsType="DISCONNECTOR"]')
    .style({
        'background-fit': 'cover',
        'background-image': 'assets/img/disconnector.png',
        'background-opacity': '0',
        'height': '65px',
        'width': '65px'
    })
    .selector('node[dmsType="LOADBREAKSWITCH"]')
    .style({
        'background-fit': 'cover',
        'background-image': 'assets/img/load-break-switch.png',
        'background-opacity': '0',
        'height': '60px',
        'width': '60px'
    })
    .selector('node[dmsType="BREAKER"]')
    .style({
        'background-fit': 'cover',
        'background-image': 'assets/img/breaker.png',
        'background-opacity': '0',
        'height': '60px',
        'width': '60px'
    })
    .selector('node[dmsType="FUSE"]')
    .style({
        'shape': 'rectangle',
        'background-color': '#2b2935',
        'background-fit': 'cover',
        'background-image': 'assets/img/fuse.png',
        'height': '20px',
        'width': '20px',
    })
    .selector('node[dmsType="ACLINESEGMENT"]')
    .style({
        'width': '2px',
        'height': '150px'
    })
    .selector('node[deviceType = "remote"]')
    .style({
        'opacity': 1.0 // change this
    })
    .selector('node')
    .style({
        'shape': 'rectangle'
    })
    .selector('edge')
    .style({
        'line-color': 'data(color)',
        'width': '2px',
        'curve-style': 'taxi',
        'taxi-direction': 'vertical',
        'taxi-turn': '15px',
        'taxi-turn-min-distance': '15px'
    })
