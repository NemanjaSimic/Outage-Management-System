import tippy from 'tippy.js';
import { SwitchCommand, SwitchCommandType } from '@shared/models/switch-command.model';

const graphTooltipBody: string =
  `<p>ID: [[id]]</p>
  <p>Type: [[type]]</p>
  <p>Name: [[name]]</p>
  <p>Mrid: [[mrid]]</p>
  <p>Description: [[description]]</p>
  <p>Device type: [[deviceType]]</p>
  <p>State: [[state]]</p>
  <p>Nominal voltage: [[nominalVoltage]]</p>`;

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

      // button - mozemo i preko document.createElement() pa appendChild()
      if (node.data('dmsType') == "LOADBREAKSWITCH" || node.data('dmsType') == "DISCONNECTOR" 
            || node.data('dmsType') == "BREAKER" || node.data('dmsType') == "FUSE") {
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