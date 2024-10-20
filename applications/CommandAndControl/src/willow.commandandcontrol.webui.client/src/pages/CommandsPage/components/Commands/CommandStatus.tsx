import { Badge } from "@willowinc/ui";

export default function CommandStatus({
  value,
  className,
}: {
  value: string;
  className?: string;
}) {
  const size = "sm";

  const statusChipPropsMap = {
    Completed: { color: "green" },
    Executing: { color: "teal" },
    Cancelled: { color: "purple" },
    Suspended: { color: "orange" },
    Approved: { color: "gray" },
    Failed: { color: "red" },
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

  const props = statusChipPropsMap[value];

  return (
    <Badge
      {...props}
      //@ts-ignore
      title={value}
      className={className}
      variant="dot"
      size={size}
    >
      {value}
    </Badge>
  );
}
