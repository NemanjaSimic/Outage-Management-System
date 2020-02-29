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
        .replace("[[defaultIsolationPoints]]", outage.DefaultIsolationPoints.join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.join(separator))
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt));
}

export const generateIsolatedOutageTemplate = (outage: ActiveOutage) => {
    return isolatedOutageTooltipContent
        .replace("[[id]]", outage.Id.toString())
        .replace("[[elementId]]", outage.ElementId.toString())
        .replace("[[defaultIsolationPoints]]", outage.DefaultIsolationPoints.join(separator))
        .replace("[[optimalIsolationPoints]]", outage.OptimalIsolationPoints.join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.join(','))
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt));
}

export const generateRepairCrewOutageTemplate = (outage: ActiveOutage) => {
    return sendRepairCrewTooltipOutage
        .replace("[[id]]", outage.Id.toString())
        .replace("[[elementId]]", outage.ElementId.toString())
        .replace("[[defaultIsolationPoints]]", outage.DefaultIsolationPoints.join(separator))
        .replace("[[optimalIsolationPoints]]", outage.OptimalIsolationPoints.join(separator))
        .replace("[[affectedConsumers]]", outage.AffectedConsumers.join(','))
        .replace("[[state]]", OutageLifeCycleState[outage.State])
        .replace("[[reportedAt]]", formatDate(outage.ReportedAt))
        .replace("[[repairedAt]]", formatDate(outage.RepairedAt));
}