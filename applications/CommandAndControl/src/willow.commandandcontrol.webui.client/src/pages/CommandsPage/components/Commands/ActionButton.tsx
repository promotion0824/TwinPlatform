import { Button, Icon, IconName } from "@willowinc/ui";
import { MouseEventHandler } from "react";
import { ResolvedCommandAction } from "../../../../services/Clients";

export default function ActionButton({
  value,
  onClick,
}: {
  value: ResolvedCommandAction;
  onClick: MouseEventHandler<HTMLButtonElement | HTMLAnchorElement>;
}) {
  const statusChipPropsMap = {
    Cancel: { icon: "close" },
    Execute: { icon: "play_arrow" },
    Suspend: { icon: "pause" },
    Unsuspend: { icon: "sync" },
    Retry: { icon: "sync" },
  } as Record<
    string,
    {
      icon: IconName;
    }
  >;

  const props = statusChipPropsMap[value];

  return (
    <Button
      title={value}
      kind="secondary"
      onClick={onClick}
      prefix={<Icon icon={props.icon} />}
    >
      {value}
    </Button>
  );
}
