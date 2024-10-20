import { Avatar } from "@willowinc/ui";
import { ActivityType, IActivityLogsResponseDto } from "../../../services/Clients";
import { getInitials } from "../../../utils/getInitials";
import { AppInitials, AppName } from "../../../utils/appName";

export const ActivityLogRole: React.FC<{ activityLog: IActivityLogsResponseDto }> = ({ activityLog }) => {
  switch (activityLog.type) {
    case ActivityType.Received:
      return <><Avatar className="mr-2" color="purple" shape="rectangle" variant="subtle" size="sm">A</Avatar>Activate</>;
    case ActivityType.Approved:
    case ActivityType.Cancelled:
    case ActivityType.Executed:
    case ActivityType.Retracted:
    case ActivityType.Suspended:
    case ActivityType.Retried:
      return <><Avatar className="mr-2" color="red" shape="rectangle" variant="subtle" size="sm">{activityLog.updatedBy?.name ? getInitials(activityLog.updatedBy?.name) : "UN"}</Avatar> {activityLog.updatedBy?.name ?? activityLog.updatedBy?.email ?? "Unknown"}</>;
    case ActivityType.MessageSent:
    case ActivityType.Completed:
    case ActivityType.Failed:
      return <><Avatar className="mr-2" color="blue" shape="rectangle" variant="subtle" size="sm">{AppInitials}</Avatar>{AppName}</>;
    case ActivityType.MessageReceivedFailed:
    case ActivityType.MessageReceivedSuccess:
      return <><Avatar className="mr-2" color="teal" shape="rectangle" variant="subtle" size="sm">ED</Avatar>Edge Device</>;
    default:
      return null;
  }
}
