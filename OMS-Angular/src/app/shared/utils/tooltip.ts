import tippy from 'tippy.js';
import { SwitchCommand, SwitchCommandType } from '@shared/models/switch-command.model';

const graphTooltipBody: string =
  `<p>ID: [[id]]</p>
  <p>Type: [[type]]</p>
  <p>State: [[state]]</p>`;

export const addGraphTooltip = (cy, node) => {
  let ref = node.popperRef();

  node.tooltip = tippy(ref, {
    content: () => {
      // node information - mozemo preko stringa da dodamo u div
      const div = document.createElement('div');
      div.innerHTML = graphTooltipBody
        .replace("[[id]]", node.data('id'))
        .replace("[[type]]", node.data('type'))
        .replace("[[state]]", node.data('state'));

      // button - mozemo i preko document.createElement() pa appendChild()
      if (node.data('type') == "Breaker" || node.data('type') == "Disconnector") {
        const button = document.createElement('button');

        if (node.data('state') == "active") {
          button.innerHTML = 'Switch off';
        }
        else {
          button.innerHTML = 'Switch on';
        }

        button.addEventListener('click', () => {

          // jer je u mocku string, a u sistemu je long
          const guid = Math.random() * 1000; 

          if (node.data('state') == "active") {
            const command: SwitchCommand = {
              guid,
              type: SwitchCommandType.TURN_OFF
            };

            node.sendSwitchCommand(command);

            node.data('state', 'inactive');
            button.innerHTML = 'Switch on';
          } else {

            const command: SwitchCommand = {
              guid,
              type: SwitchCommandType.TURN_ON
            };

            node.sendSwitchCommand(command);

            node.data('state', 'active');
            button.innerHTML = 'Switch off';
          }

        });

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