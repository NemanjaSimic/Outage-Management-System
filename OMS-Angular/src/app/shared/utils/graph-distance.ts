export const modifyNodeDistance = (nodes) => {
    nodes.forEach(node => {
        node.position().y -= 60;
    });
}