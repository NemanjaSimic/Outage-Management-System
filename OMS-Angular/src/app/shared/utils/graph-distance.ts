export const modifyNodeDistance = (nodes) => {
    nodes.forEach(node => {
        console.log(node.position());
        node.position().y -= 60;
        console.log(node.position());
    });
}