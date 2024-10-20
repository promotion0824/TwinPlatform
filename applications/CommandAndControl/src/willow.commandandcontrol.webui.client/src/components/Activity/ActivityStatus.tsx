import { Badge } from "@willowinc/ui";
import { ActivityType } from "../../services/Clients";

export function ActivityStatus({
  activityType,
  className,
}: {
  activityType: ActivityType;
  className?: string;
}) {
  const size = "sm";

  const statusChipPropsMap = {
    [ActivityType.Received]: { color: "gray" },
    [ActivityType.Approved]: { color: "gray" },
    [ActivityType.Failed]: { color: "red" },
    [ActivityType.Executed]: { color: "teal" },
    [ActivityType.Cancelled]: { color: "yellow" },
    [ActivityType.Suspended]: { color: "gray" },
    [ActivityType.Retracted]: { color: "pink" },
  } as Record<
    string,
    {
      color:
        | "gray"
        | "red"
        | "pink"
        | "blue"
        | "cyan"
        | "green"
        | "yellow"
        | "orange"
        | "teal"
        | "purple";
    }
  >;

  const props = statusChipPropsMap[activityType];

  return (
    <Badge
      {...props}
      //@ts-ignore
      title={activityType}
      className={className}
      variant="dot"
      size={size}
    >
      {activityType}
    </Badge>
  );
}
