import * as cytoscape from 'cytoscape';

export const style = cytoscape.stylesheet()
    .selector('node[state = "active"]')
    .style({
        'label': 'data(type)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'background-color': 'green'
    })
    .selector('node[state = "inactive"]')
    .style({
        'label': 'data(type)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'background-color': 'red'
    })
    .selector('node[type = "warning"]')
    .style({
        'shape': 'rectangle',
        'background-color': '#2b2935',
        'background-fit': 'cover',
        'background-image': 'assets/img/warning.png',
        'height': '20px',
        'width': '20px',
    })
    .selector('node[type="ENERGYSOURCE"]')
    .style({
        'label': 'data(type)',
        'text-valign': 'center',
        'text-halign': 'center',
        'rotate':'90',
        'shape': 'triangle',
        'background-color': 'green'
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
        'label': 'data(type)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'width': '2px',
        'background-color': 'red'
    })
    .selector('edge')
    .style({
        'line-color': 'data(color)',
        'width': '2px',
        'curve-style': 'taxi',
        'taxi-direction': 'vertical',
        'taxi-turn': '15px',
        'taxi-turn-min-distance': '10px'
    })