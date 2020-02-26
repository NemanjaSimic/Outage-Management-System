import { Node } from "./node.model";

export interface BreakerNode extends Node {
  isClosed: boolean;
}