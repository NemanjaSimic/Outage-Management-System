export const drawBackupEdge = (cy, line) => {
    cy.add([
        {
            group: "edges",
            data: {
                id: 'backup',
                source: line.data.source,
                target: line.data.target,
                color: line.data.color
            }
        }
    ]);
}