import { Node } from "./node.model";

export interface TransformerNode extends Node {
  firstWinding: Node;
  secondWinding: Node;
}