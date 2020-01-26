import uuid from 'uuid';

export const drawCallWarning = (cy, gid: Number) => {
  console.log(`Outage Gid: ${gid}`);

  cy.nodes().forEach(node => {
    if (node.data('id') == gid) {
      const nodePosition = node.position();

      cy.add([
        {
          group: "nodes",
          data: {
            id: `${uuid()}`,
            type: 'outage-call'
          },
          // position should be changed to be left/right of node
          position: {
            x: nodePosition.x + 30,
            y: nodePosition.y
          }
        }
      ]);

    }
  })

};