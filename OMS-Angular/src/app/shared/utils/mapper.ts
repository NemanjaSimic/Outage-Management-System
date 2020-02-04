const POWERTRANSFORMER_DMSTYPE = "POWERTRANSFORMER";

const mappingRules = {
  "regular": (node) => mapRegularNode(node),
  "transformer": (node) => mapTransformerNode(node)
};

export const mapNode = (node) => {
  const rule = node.DMSType == POWERTRANSFORMER_DMSTYPE
    ? "transformer"
    : "regular";

  return mappingRules[rule](node);
}

export const mapRelation = (relation) => {
  return {
    data: {
      source: relation.SourceNodeId,
      target: relation.TargetNodeId,
      color: relation.IsActive ? "green" : "blue"
    }
  }
}

const mapRegularNode = (node) => {
  return {
    data: {
      id: node.Id,
      name: node.Name,
      description: node.Description,
      mrid: node.Mrid,
      state: node.IsActive ? "active" : "inactive",
      dmsType: node.DMSType,
      measurements: node.Measurements,
      nominalVoltage: node.NominalVoltage,
      deviceType: node.IsRemote ? "remote" : "local"
    }
  }
}

const mapTransformerNode = (node) => {
  return {
    data: {
      id: node.Id,
      name: node.Name,
      description: node.Description,
      mrid: node.Mrid,
      state: node.IsActive ? "active" : "inactive",
      dmsType: node.DMSType,
      measurements: node.Measurements,
      nominalVoltage: node.NominalVoltage,
      deviceType: node.IsRemote ? "remote" : "local",
      firstWinding: node.FirstWinding,
      secondWinding: node.SecondWinding
    }
  }
}