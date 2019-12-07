import * as cytoscape from 'cytoscape';

export const style = cytoscape.stylesheet()
    .selector('node[state = "active"]')
    .style({
        'label': 'data(id)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'background-color': 'green'
    })
    .selector('node[state = "inactive"]')
    .style({
        'label': 'data(id)',
        'text-valign': 'center',
        'text-halign': 'center',
        'shape': 'rectangle',
        'background-color': 'red'
    })
    .selector('edge')
    .style({
        'line-color': 'data(color)',
        'curve-style': 'taxi',
        'taxi-direction': 'downward',
        'taxi-turn-min-distance': '5px'
    })