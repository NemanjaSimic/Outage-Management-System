import uuid from 'uuid';

/* export const drawWarning = (cy, line) => {
  if (line.data('color') == 'red') {
    const source = line.sourceEndpoint();
    const target = line.targetEndpoint();

    // ako je levo, onda je verovatno prvi, pa da iscrta sa leve strane
    const isFirstChild = source.x > target.x;

    cy.add([
      {
        group: "nodes",
        data: {
          id: `${uuid()}`,
          type: 'warning',
          targetId: line.data('target')
        },
        position: {
          x: isFirstChild ? target.x - 15 : target.x + 15,
          y: target.y - 25
        }
      }
    ]);
  }
} */

export const drawWarning = (cy, node) => {
  cy.add([
      {
        group: "nodes",
        data: {
          id: `${uuid()}`,
          type: 'warning'
      },
      position: {
          x: node.position("x") + 20,
          y: node.position("y") - 25
      }
      }
  ]);
}