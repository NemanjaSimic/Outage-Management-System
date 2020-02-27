import uuid from 'uuid';

export const drawWarningOnLine = (cy, line) => {
  if(!line) return;

  const id = uuid();
  
  if (line.data('color') == 'red') {
    const source = line.sourceEndpoint();
    const target = line.targetEndpoint();

    //ako je levo, onda je verovatno prvi, pa da iscrta sa leve strane
    const isFirstChild = source.x > target.x;

    cy.add([
      {
        group: "nodes",
        data: {
          id,
          type: 'warning',
          targetId: line.data('target')
        },
        position: {
          x: isFirstChild ? target.x - 25 : target.x + 25,
          y: target.y - 25
        }
      }
    ]);
  }

  return id;
}

export const drawWarningOnNode = (cy, node) => {
  if(!node) return;

  const id = uuid();

  cy.add([
      {
        group: "nodes",
        data: {
          id,
          type: 'warning'
      },
      position: {
          x: node.position("x") + 60,
          y: node.position("y")
      }
      }
  ]);

  return id;
}