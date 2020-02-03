import * as cytoscape from 'cytoscape';

export const style = cytoscape.stylesheet()
    .selector('node[state = "active"]')
    .style({
        'label': 'data(dmsType)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'background-color': 'green'
    })
    .selector('node[state = "inactive"]')
    .style({
        'label': 'data(dmsType)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'background-color': 'blue'
    })
    .selector('node[dmsType="ENERGYCONSUMER"]')
    .style({
        'label': '',
        'background-fit': 'cover',
        'background-opacity': '0',
        'background-image': 'assets/img/consumer.png',
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
        'label': '',
        'background-fit': 'cover',
        'background-image': 'assets/img/energy2.png',
        'background-opacity': '0',
        'height': '100px',
        'width': '100px'
    })
    .selector('node[dmsType="POWERTRANSFORMER"]')
    .style({
        'label': '',
        'background-fit': 'cover',
        'background-image': 'assets/img/power-transformer.png',
        'background-opacity': '0',
        'height': '100px',
        'width': '100px'
    })
    .selector('node[dmsType="FUSE"]')
    .style({
        'label': '',
        'background-fit': 'cover',
        'background-image': 'assets/img/fuse.png',
        'background-opacity': '0',
        'height': '35px',
        'width': '35px'
    })
    .selector('node[dmsType="DISCONNECTOR"]')
    .style({
        'label': '',
        'background-fit': 'cover',
        'background-image': 'assets/img/disconnector.png',
        'background-opacity': '0',
        'height': '65px',
        'width': '65px'
    })
    .selector('node[dmsType="LOADBREAKSWITCH"]')
    .style({
        'label': '',
        'background-fit': 'cover',
        'background-image': 'assets/img/load-break-switch.png',
        'background-opacity': '0',
        'height': '60px',
        'width': '60px'
    })
    .selector('node[dmsType="BREAKER"]')
    .style({
        'label': '',
        'background-fit': 'cover',
        'background-image': 'assets/img/breaker.png',
        'background-opacity': '0',
        'height': '60px',
        'width': '60px'
    })
    .selector('node[type="FUSE"]')
    .style({
        'shape': 'rectangle',
        'background-color': '#2b2935',
        'background-fit': 'cover',
        'background-image': 'assets/img/fuse.png',
        'height': '20px',
        'width': '20px',
    })
    .selector('node[type="ACLINESEGMENT"]')
    .style({
        'opacity': 1.0 // change this
    })
    // .selector('node[deviceType = "remote"]')
    // .style({
    //     'label': 'data(dmsType)',
    //     'text-valign': 'center',
    //     'text-halign': 'center',
    //     'shape': 'rectangle',
    //     'background-color': 'green',
    //     'opacity': 0.7
    // })
    .selector('edge')
    .style({
        'line-color': 'data(color)',
        'width': '2px',
        'curve-style': 'taxi',
        'taxi-direction': 'vertical',
        'taxi-turn': '15px',
        'taxi-turn-min-distance': '15px'
    })
