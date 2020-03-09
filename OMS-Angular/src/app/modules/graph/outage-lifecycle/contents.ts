import {
    createdOutageTooltipContent,
    isolatedOutageTooltipContent,
    sendRepairCrewTooltipOutage
} from './templates';
import { ActiveOutage, OutageLifeCycleState } from '@shared/models/outage.model';
import { formatDate } from '@shared/utils/date';

const separator = ',';

export const generateCreatedOutageTemplate = (outage: ActiveOutage) => {
    return createdOutageTooltipContent
        .replace("[[id]]", outage.Id.toString())
        .replace("[[defaultIsolationPoints]]", outage.DefaultIsolationPoints.map(point => `<br/>${point.Mrid} (0x${point.Id.toString(16).toUpperCase()})`).join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.length.toString())
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt));
}

export const generateIsolatedOutageTemplate = (outage: ActiveOutage) => {
    return isolatedOutageTooltipContent
        .replace("[[id]]", outage.Id.toString())
        .replace("[[elementId]]", `0x${outage.ElementId.toString(16)}`)
        .replace("[[defaultIsolationPoints]]", outage.DefaultIsolationPoints.map(point => `<br/>${point.Mrid} (0x${point.Id.toString(16).toUpperCase()})`).join(separator))
        .replace("[[optimalIsolationPoints]]", outage.OptimalIsolationPoints.map(point => `<br/>${point.Mrid} (0x${point.Id.toString(16).toUpperCase()})`).join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.length.toString())
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt));
}

export const generateRepairCrewOutageTemplate = (outage: ActiveOutage) => {
    return sendRepairCrewTooltipOutage
        .replace("[[id]]", outage.Id.toString())
        .replace("[[elementId]]", `0x${outage.ElementId.toString(16)}`)
        .replace("[[defaultIsolationPoints]]", outage.DefaultIsolationPoints.map(point => `<br/>${point.Mrid} (0x${point.Id.toString(16).toUpperCase()})`).join(separator))
        .replace("[[optimalIsolationPoints]]", outage.OptimalIsolationPoints.map(point => `<br/>${point.Mrid} (0x${point.Id.toString(16).toUpperCase()})`).join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.length.toString())
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt))
        .replace("[[repairedAt]]", formatDate(outage.RepairedAt));
}