import { Button, Icon, IconName } from "@willowinc/ui";
import { MouseEventHandler } from "react";
import styled from "styled-components";

export const ActionButton: React.FC<ActionButtonProps> =({value, onClick, loading, selected, locked}) => {

  const statusChipPropsMap = {
    Approve: { icon: "check", kind: "secondary" },
    Reject: { icon: "close", kind: "secondary" },
    Approved: { kind: "primary", },
    Rejected: { kind: "negative",},
  } as Record<string,
    {
      icon?: IconName;
      kind?: "primary" | "secondary" | "negative";
      disabled?: boolean;
    }>;

  const props = statusChipPropsMap[value];

  return (
    <StyledButton
      loading={loading}
      title={value}
      kind={props.kind}
      disabled={!selected && locked}
      onClick={!locked ? onClick : undefined}
      prefix={props.icon && <Icon icon={props.icon} />}
    >
      {value}
    </StyledButton>
  );
};

const StyledButton = styled(Button)`
width: 100px;
`;

export interface ActionButtonProps {
  value: string;
  onClick: MouseEventHandler<HTMLButtonElement | HTMLAnchorElement>;
  loading: boolean;
  selected: boolean;
  locked: boolean;
}
