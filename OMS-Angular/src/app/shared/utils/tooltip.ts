import tippy from 'tippy.js';
import 'tippy.js/themes/light-border.css';

const graphTooltipBody: string =
`<p>ID: [[id]]</p>
<p>State: [[state]]</p>`;

export const addGraphTooltip = (node) => {
  let ref = node.popperRef();

  node.tippy = tippy(ref, {
    content: () => {
      const div = document.createElement('div');

      div.innerHTML = graphTooltipBody
        .replace("[[id]]", node.data('id'))
        .replace("[[state]]", node.data('state'));

      return div;
    },
    theme: 'light-border',
    animation: 'perspective',
    trigger: 'manual',
    placement: 'right',
    arrow: true
  });

  node.on('tap', () => {
    // nemam pojma zasto ovako radi, ali radi ...
    setTimeout(() => {
      node.tippy.show();
    }, 0);
  });
}