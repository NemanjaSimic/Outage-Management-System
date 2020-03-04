const POWERTRANSFORMER_DMSTYPE = "POWERTRANSFORMER";
const BREAKER_DMSTYPE = "BREAKER";

const mappingRules = {
  "regular": (node) => mapRegularNode(node),
  "transformer": (node) => mapTransformerNode(node),
  "breaker": (node) => mapBreakerNode(node)
};

export const mapNode = (node) => {
  const rule = 
    node.DMSType == POWERTRANSFORMER_DMSTYPE ? "transformer" :
    node.DMSType == BREAKER_DMSTYPE ? "breaker" :
    "regular";

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

const mapBreakerNode = (node) => {
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
      isClosed: 
        (node.Measurements[0] != null && node.Measurements[0] != undefined) ? 
          (node.Measurements[0].Value != 0 ? "closed" : "open") : "open",
      isRecloser: node.NoReclosing ? "no" : "yes"
  }
}
}