import tippy from 'tippy.js';

const graphTooltipBody: string =
`<p>ID: [[id]]</p>
<p>State: [[state]]</p>`;

export const addGraphTooltip = (node) => {
  let ref = node.popperRef();

  node.tooltip = tippy(ref, {
    content: () => {
      // node information - mozemo preko stringa da dodamo u div
      const div = document.createElement('div');
      div.innerHTML = graphTooltipBody
        .replace("[[id]]", node.data('id'))
        .replace("[[state]]", node.data('state'));

      // button - mozemo i preko document.createElement() pa appendChild()
      const button = document.createElement('button');
      button.innerHTML = "Switch on";
      button.addEventListener('click', () => {
        console.log('switching on node with id: ', node.data('id'));
      });

      div.appendChild(button);
      
      return div;
    },
    animation: 'perspective',
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
}