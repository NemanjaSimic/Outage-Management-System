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
        'label': 'data(dmsType)',
        'text-valign': 'center',
        'text-halign': 'center',
        'rotate': '90',
        'shape': 'triangle',
        'background-color': 'green'
    })
    .selector('node[dmsType="ACLINESEGMENT"]')
    .style({
        'label': 'data(dmsType)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'width': '2px',
        'height': '100px',
        'background-color': 'green'
    })
    .selector('node[deviceType = "remote"]')
    .style({
        'opacity': 1.0 // change this
    })
    .selector('edge')
    .style({
        'line-color': 'green',
        'width': '2px',
        'curve-style': 'taxi',
        'taxi-direction': 'vertical',
        'taxi-turn': '15px',
        'taxi-turn-min-distance': '15px'
    })
