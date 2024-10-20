import { Badge } from "@willowinc/ui";
import format from "date-fns/format";
import { ActivityStatus } from "../../../components/Activity/ActivityStatus";
import { ActivityType, IActivityLogsResponseDto } from "../../../services/Clients";
import { formatDate } from "../../../utils/formatDate";

export const ActivityLogMessage: React.FC<{ activityLog: IActivityLogsResponseDto }> = ({ activityLog }) => {
  switch (activityLog.type) {
    case ActivityType.Received:
      return (
        <span>Command received to set value <strong>{activityLog.value}{activityLog.unit}</strong> from <strong>{format(activityLog.startTime!, "HH:mm")}</strong>
          {!!activityLog.endTime && <> until <strong>{format(activityLog.endTime!, "HH:mm")}</strong></>}</span>
      );
    case ActivityType.Approved:
    case ActivityType.Cancelled:
    case ActivityType.Executed:
    case ActivityType.Failed:
    case ActivityType.Retracted:
    case ActivityType.Completed:
    case ActivityType.Suspended:
    case ActivityType.Retried:
      return <><ActivityStatus activityType={activityLog.type!} className="mr-2" /> Command was {activityLog.type.toString().toLowerCase()}</>;
    case ActivityType.MessageSent:
      return <span>Write request <strong>{activityLog.value}{activityLog.unit}</strong> sent for {formatDate(activityLog.startTime!)}</span>;
    case ActivityType.MessageReceivedFailed:
      return <span>Write request status <Badge color="red" variant="bold">Failed</Badge></span>
    case ActivityType.MessageReceivedSuccess:
      return <span>Write request status <Badge color="green" variant="bold">Success</Badge></span>
    default:
      return null;
  }
}
