import { Node } from 'reactflow';
import { IBoundingBox } from './GraphBase';

const oval = 5;

/**
 * Distance between node and position (squared)
 */
const distance = (a: Node<any>, b: { x: number, y: number }) =>
    (a.position.x - b.x) ** 2 + (a.position.y - b.y) ** 2;

/**
 * Pseudo-Distance between two Nodes with a horizontal bias
 */
const hdistance = (a: Node<any>, b: Node<any>) =>
    (a.position.x - b.position.x) ** 2 + oval * (a.position.y - b.position.y) ** 2;

/**
* Pseudo-Distance between two Nodes with a vertical bias
*/
const vdistance = (a: Node<any>, b: Node<any>) =>
    oval * (a.position.x - b.position.x) ** 2 + (a.position.y - b.position.y) ** 2;

export interface Itransition {
    <TNode extends Node<any>>(nodes: TNode[], arg: TNode): TNode;
}

/**
 * Find the node closest to being to the right of the current node
 */
const goRight: Itransition = <TNode extends Node<any>>(nodes: TNode[], input: TNode): TNode => {
    var node: TNode = input ?? nodes[0];
    var closest: TNode = node;
    nodes.forEach((n, i) => {
        if (n.position.x <= node.position.x) return;
        if (closest.id === node.id) {
            // Anything right of here is right when we have nothing
            closest = n;
        }
        else {
            // But if we have something, can we find something closer? (oval bias)
            const dist1 = hdistance(n, node);
            const dist2 = hdistance(closest, node);
            if (dist1 < dist2) {
                closest = n;
            }
        }
    });
    return closest;
};

/**
 * Find the node closest to being to the left of the current node
 */
const goLeft: Itransition = <TNode extends Node<any>>(nodes: TNode[], input: TNode): TNode => {
    // Find the node closest to being to the left of the current node
    var node: TNode = input ?? nodes[0];
    var closest: TNode = node;
    nodes.forEach((n, i) => {
        if (n.position.x >= node.position.x) return;
        if (closest.id === node.id) {
            // Anything left of here is right when we have nothing
            closest = n;
        }
        else {
            // But if we have something, can we find something closer?
            const dist1 = hdistance(n, node);
            const dist2 = hdistance(closest, node);
            if (dist1 < dist2) {
                closest = n;
            }
        }
    });
    return closest;
};

/**
 * Find the node closest to being below the current node
 */
const goDown: Itransition = <TNode extends Node<any>>(nodes: TNode[], input: TNode): TNode => {
    var node: TNode = input ?? nodes[0];
    var closest: TNode = node;
    nodes.forEach((n, i) => {
        if (n.position.y <= node.position.y) return;
        if (closest.id === node.id) {
            // Anything right of here is right when we have nothing
            closest = n;
        }
        else {
            // But if we have something, can we find something closer? (oval bias)
            const dist1 = vdistance(n, node);
            const dist2 = vdistance(closest, node);
            if (dist1 < dist2) {
                closest = n;
            }
        }
    });
    return closest;
};

/**
 * Find the node closest to being above the current node
 */
const goUp: Itransition = <TNode extends Node<any>>(nodes: TNode[], input: TNode): TNode => {
    var node: TNode = input ?? nodes[0];
    var closest: TNode = node;
    nodes.forEach((n, i) => {
        if (n.position.y >= node.position.y) return;
        if (closest.id === node.id) {
            // Anything left of here is right when we have nothing
            closest = n;
        }
        else {
            // But if we have something, can we find something closer?
            const dist1 = vdistance(n, node);
            const dist2 = vdistance(closest, node);
            if (dist1 < dist2) {
                closest = n;
            }
        }
    });
    return closest;
};

/**
 * Find closest node to the center of the bounding box
 */
const goCenter = <TNode extends Node<any>>(nodes: TNode[], bounds: IBoundingBox): TNode => {
    const newCenter = { x: bounds.maxX / 2, y: bounds.maxY / 2 };
    var closest: TNode = nodes[0];
    nodes.forEach((n, i) => {
        const dist1 = distance(closest, newCenter);
        const dist2 = distance(n, newCenter);
        if (dist2 < dist1) closest = n;
    });
    return closest;
}

const boxWidth = 70;
const boxHeight = 40;

const reducer = (b: IBoundingBox, e: any, i: number, a: any[]): IBoundingBox =>
(
    e.position ?
        {
            // TODO: What is size of each box? 70 x 40 ?
            minX: Math.min(e.position.x, b.minX), maxX: Math.max(e.position.x + boxWidth, b.maxX),
            minY: Math.min(e.position.y, b.minY), maxY: Math.max(e.position.y + boxHeight, b.maxY)
        } : b
);

export { hdistance, vdistance, goLeft, goRight, goUp, goDown, goCenter, boxWidth, boxHeight, reducer };
