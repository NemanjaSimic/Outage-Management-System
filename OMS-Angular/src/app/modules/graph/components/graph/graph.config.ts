import { style } from './graph.style';

const cyConfig: Object = {
  layout: { name: 'dagre', rankDir: 'TB' },
  autoungrabify: true,
  style: style,
  wheelSensitivity: 0.1,
  minZoom: 1,
  maxZoom: 4
};

export default cyConfig;