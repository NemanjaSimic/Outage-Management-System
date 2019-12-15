import { Node } from '@shared/models/node.model'
import { Relation } from '@shared/models/relation.model'

export const mapNode = (node: Node) => {
  return {
    data: {
      id: node.Id,
      label: node.Name,
      state: node.IsActive ? "active" : "inactive"
    }
  }
}

export const mapRelation = (relation: Relation) => {
  return {
    data: {
      source: relation.SourceNodeId,
      target: relation.TargetNodeId,
      color: relation.IsActive ? "blue" : "red"
    }
  }
}