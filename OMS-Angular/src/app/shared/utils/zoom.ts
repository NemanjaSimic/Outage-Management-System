export const zoom = (cy, id: string) => {
  cy.nodes().forEach(node => {
    if(node.data('id') == id){
      cy.reset();   
      cy.zoom({
        level: 2,
        position: {
          x: node.position().x,
          y: node.position().y
        }
      });

      setTimeout(() => node.tooltip.show(), 0);
    }
  })
};