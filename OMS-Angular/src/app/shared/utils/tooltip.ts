import tippy from 'tippy.js';
import { SwitchCommand, SwitchCommandType } from '@shared/models/switch-command.model';

const commandableTypes: string[] = ["LOADBREAKSWITCH", "DISCONNECTOR", "BREAKER", "FUSE"];

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
  `<p>ID: [[id]]</p>
  <p>ElementID: [[elementId]]</p>
  <p>ReportTime: [[reportTime]]</p>
  <p>ArchiveTime: [[archiveTime]]</p>`;

export const addGraphTooltip = (cy, node) => {
  let ref = node.popperRef();

  node.tooltip = tippy(ref, {
    content: () => {
      // node information - mozemo preko stringa da dodamo u div
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
        if(meas.length > 0){
          if (meas[0].Value == 0) {
            button.innerHTML = 'Switch off';
          }
          else {
            button.innerHTML = 'Switch on';
          }

          button.addEventListener('click', () => {
            // jer je u mocku string, a u sistemu je long       
            const guid = meas[0].Id;
            if (meas[0].Value == 0) {
            const command: SwitchCommand = {
                guid,
                command: SwitchCommandType.TURN_OFF
              };

              node.sendSwitchCommand(command);

            } else {

              const command: SwitchCommand = {
                guid,
                command: SwitchCommandType.TURN_ON
              };

              node.sendSwitchCommand(command);
              }
            });
          }
          div.appendChild(button);
        }

      return div;
    },
    animation: 'scale',
    trigger: 'manual',
    placement: 'right',
    arrow: true,
    interactive: true
  });

  node.on('tap', () => {
    // nemam pojma zasto ovako radi, ali radi ...
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
        .replace("[[reportTime]]", outage["data"]['reportTime'])
        .replace("[[archiveTime]]", outage["data"]['archiveTime']);
    
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

  cy.on('zoom pan', () => {
    setTimeout(() => {
      node.tooltip.hide();
    }, 0);
  });
}