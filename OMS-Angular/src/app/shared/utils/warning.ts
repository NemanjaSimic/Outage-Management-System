import uuid from 'uuid';

export const drawWarning = (line) => {
  if (line.data('color') == 'red') {
    const source = line.sourceEndpoint();
    const target = line.targetEndpoint();

    // ako je levo, onda je verovatno prvi, pa da iscrta sa leve strane
    const isFirstChild = source.x > target.x;

    this.cy.add([
      {
        group: "nodes",
        data: {
          id: `${uuid()}`,
          type: 'warning'
        },
        position: {
          x: isFirstChild ? target.x - 15 : target.x + 15,
          y: target.y - 25
        }
      }
    ]);
  }
}