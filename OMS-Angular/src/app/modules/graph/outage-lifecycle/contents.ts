import {
    createdOutageTooltipContent,
    isolatedOutageTooltipContent,
    sendRepairCrewTooltipOutage
} from './templates';
import { ActiveOutage, OutageLifeCycleState } from '@shared/models/outage.model';
import { formatDate } from '@shared/utils/date';

const separator = ', ';

export const generateCreatedOutageTemplate = (outage: ActiveOutage) => {
    return createdOutageTooltipContent
        .replace("[[id]]", outage.Id.toString())
        .replace("[[defaultIsolationPoints]]", "0x" + (outage.DefaultIsolationPoints.map(point => point.toString(16))).join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.length.toString())
        // .replace("[[affectedConsumers]]", outage.AffectedConsumers.map(aff => aff.Id).join(separator))
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt));
}

export const generateIsolatedOutageTemplate = (outage: ActiveOutage) => {
    return isolatedOutageTooltipContent
        .replace("[[id]]", outage.Id.toString())
        .replace("[[elementId]]", "0x" + outage.ElementId.toString(16))
        .replace("[[defaultIsolationPoints]]", "0x" + (outage.DefaultIsolationPoints.map(point => point.toString(16))).join(separator))
        .replace("[[optimalIsolationPoints]]", "0x" + (outage.OptimalIsolationPoints.map(point => point.toString(16))).join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.length.toString())
        // .replace("[[affectedConsumers]]", outage.AffectedConsumers.map(aff => aff.Id).join(separator))
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt));
}

export const generateRepairCrewOutageTemplate = (outage: ActiveOutage) => {
    return sendRepairCrewTooltipOutage
        .replace("[[id]]", outage.Id.toString())
        .replace("[[elementId]]", "0x" + outage.ElementId.toString(16))
        .replace("[[defaultIsolationPoints]]", "0x" + (outage.DefaultIsolationPoints.map(point => point.toString(16))).join(separator))
        .replace("[[optimalIsolationPoints]]", "0x" + (outage.OptimalIsolationPoints.map(point => point.toString(16))).join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.length.toString())
        // .replace("[[affectedConsumers]]", outage.AffectedConsumers.map(aff => aff.Id).join(separator))
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt))
        .replace("[[repairedAt]]", formatDate(outage.RepairedAt));
}