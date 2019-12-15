import { Node } from './node.model';
import { Relation } from './relation.model';

export interface OmsGraph {
  Nodes: Node[];
  Relations: Relation[];
}