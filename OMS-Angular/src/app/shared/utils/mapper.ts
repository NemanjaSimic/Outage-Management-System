import { Node } from '@shared/models/node.model'
import { Relation } from '@shared/models/relation.model'

export const mapNode = (node: Node) => {
  return {
    data: {
      id: node.Id,
      state: node.IsActive ? "active" : "inactive",
      dmsType: node.DMSType,
      measurementType: node.MeasurementType,
      measurementValue: node.MeasurementValue,
      nominalVoltage: node.NominalVoltage,
      deviceType: node.IsRemote ? "remote" : "local"
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