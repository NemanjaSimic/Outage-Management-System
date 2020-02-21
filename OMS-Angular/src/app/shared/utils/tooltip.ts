import tippy from 'tippy.js';
import { SwitchCommand, SwitchCommandType } from '@shared/models/switch-command.model';
//import { GetUnitMeasurement } from './measurement';

const commandableTypes: string[] = ["LOADBREAKSWITCH", "DISCONNECTOR", "BREAKER", "FUSE"];

// global var - lose (trebali bi naci drugacije resenje)
// mozda da cuvamo u komponenti, pa da prosledjujemo
let commandedNodeIds: string[] = []; 

const graphTooltipBody: string =
  `<p>ID: [[id]]</p>
  <p>Type: [[type]]</p>
  <p>Name: [[name]]</p>
  <p>Mrid: [[mrid]]</p>
  <p>Description: [[description]]</p>
  <p>Device type: [[deviceType]]</p>
  <p>State: [[state]]</p>
  <p>Nominal voltage: [[nominalVoltage]]</p>`;

const outageTooltipBody: string =
  `<p>OutageID: [[id]]</p>
  <p>ElementID: [[elementId]]</p>
  <p>State: [[state]]</p>
  <p>ReportedTime: [[reportedAt]]</p>`;

/* const measurementsToolTipBody: string =
`<h3>MEASUREMENTS</h3>
<p>Type: [[type]] Value:[[value]] [[unit]]</p>`;
 */
export const addGraphTooltip = (cy, node) => {
  let ref = node.popperRef();
  
  node.tooltip = tippy(ref, {
    content: createTooltipContent(node),
    animation: 'scale',
    trigger: 'manual',
    placement: 'right',
    arrow: true,
    interactive: true
  });
  
  tippy.hideAll();
  setTimeout(() => {
    if(commandedNodeIds.includes(node.data('id'))) {
      node.tooltip.setContent(createTooltipContent(node));
      node.tooltip.show();
      commandedNodeIds = commandedNodeIds.filter(c => c != node.data('id'));
    }
  }, 0);

  node.on('tap', () => {
    setTimeout(() => {
      node.tooltip.show();
    }, 0);
  });

  // hide the tooltip on zoom and pan
  cy.on('zoom pan', () => {
    setTimeout(() => {
      node.tooltip.hide();
    }, 0);
  });
}

const createTooltipContent =  (node) => {
  const div = document.createElement('div');
  div.innerHTML = graphTooltipBody
    .replace("[[id]]", (+node.data('id')).toString(16))
    .replace("[[type]]", node.data('dmsType'))
    .replace("[[name]]", node.data('name'))
    .replace("[[mrid]]", node.data('mrid'))
    .replace("[[description]]", node.data('description'))
    .replace("[[deviceType]]", node.data('deviceType'))
    .replace("[[state]]", node.data('state'))
    .replace("[[nominalVoltage]]", node.data('nominalVoltage'));

  if (commandableTypes.includes(node.data('dmsType'))) {
    const button = document.createElement('button');

    const meas = node.data('measurements');
    if (meas.length > 0) {
      if (meas[0].Value == 0) {
        button.innerHTML = 'Switch off';
      }
      else {
        button.innerHTML = 'Switch on';
      }

      button.addEventListener('click', () => {
        const guid = meas[0].Id;
        if (meas[0].Value == 0) {
          const command: SwitchCommand = {
            guid,
            command: SwitchCommandType.TURN_OFF
          };
          node.sendSwitchCommand(command);
          commandedNodeIds.push(node.data('id'));
        } else {
          const command: SwitchCommand = {
            guid,
            command: SwitchCommandType.TURN_ON
          };
          node.sendSwitchCommand(command);
          commandedNodeIds.push(node.data('id'));
        }
      });
    }
    div.appendChild(button);
  }

  return div;
};

export const addOutageTooltip = (cy, node, outage) => {
  if(outage == undefined)
  {
    return;
  }
  
  let ref = node.popperRef();

  node.tooltip = tippy(ref, {
    content: () => {
      const div = document.createElement('div');
      div.innerHTML = outageTooltipBody
        .replace("[[id]]", outage["data"]['id'])
        .replace("[[elementId]]", outage["data"]['elementId'])
        .replace("[[state]]", outage["data"]['state'])
        .replace("[[reportedAt]]", outage["data"]['reportedAt']);
    
        return div;
      },
      animation: 'scale',
      trigger: 'manual',
      placement: 'right',
      arrow: true,
      interactive: true
  });

  node.on('tap', () => {
    setTimeout(() => {
      node.tooltip.show();
    }, 0);
  });
}

/* export const addMeasurementTooltip = (cy, node) => {
  let ref = node.popperRef();
  let measurements = node.data("measurements");
  if(measurements == undefined)
    return;

  node.tooltip = tippy(ref, {
    content: () => {
      const div = document.createElement('div');
      measurements.forEach(element => {
        div.innerHTML = measurementsToolTipBody
        .replace("[[type]]", element.Type)
        .replace("[[value]]", element.Value)
        .replace("[[unit]]", GetUnitMeasurement(element.Type))    
      });  
      return div;
    },
    animation: 'scale',
    trigger: 'manual',
    placement: 'left',
    arrow: true,
    interactive: true
  })

  node.on('tap', () => {
    setTimeout(() => {
      node.tooltip.show();
    }, 0);
  });
} */

export const addEdgeTooltip = (cy, node, edge) => {
  let ref = edge.popperRef();
  edge.nodeId = node.data('id');

  edge.tooltip = tippy(ref, {
    content: () => {
      const div = document.createElement('div');
      div.innerHTML = graphTooltipBody
        .replace("[[id]]", (+node.data('id')).toString(16))
        .replace("[[type]]", node.data('dmsType'))
        .replace("[[name]]", node.data('name'))
        .replace("[[mrid]]", node.data('mrid'))
        .replace("[[description]]", node.data('description'))
        .replace("[[deviceType]]", node.data('deviceType'))
        .replace("[[state]]", node.data('state'))
        .replace("[[nominalVoltage]]", node.data('nominalVoltage'));

      return div;
    },
    animation: 'scale',
    trigger: 'manual',
    placement: 'right',
    arrow: true,
    interactive: true
  });  

  edge.unbind('tap');
  edge.on('tap', () => {
    setTimeout(() => {
      edge.tooltip.show();
    }, 0);
  });

  // hide the tooltip on zoom and pan
  cy.on('zoom pan', () => {
    setTimeout(() => {
      edge.tooltip.hide();
    }, 0);
  });
}; 
