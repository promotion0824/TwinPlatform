//eslint-disable-next-line @typescript-eslint/no-unused-vars

import { IValidationResults, Result, CheckType } from '../services/Clients';

import { Badge, Icon } from '@willowinc/ui';

type ChipPropsType = Record<
  string,
  {
    color: 'gray' | 'red' | 'pink' | 'blue' | 'cyan' | 'green' | 'yellow' | 'orange' | 'teal' | 'purple';
    variant: 'bold' | 'muted' | 'outline' | 'subtle' | 'dot';
    children: React.ReactNode;
    size: 'xs' | 'sm' | 'md' | 'lg';
    prefix: React.ReactNode;
  }
>;

export const DqResultTypeChip = ({ row }: { row: IValidationResults }) => {
  const size = 'sm';

  const statusChipPropsMap = {
    [Result.Ok + CheckType.Properties]: {
      color: 'green',
      variant: 'bold',
      children: 'Property',
      prefix: <Icon icon="check_circle" />,
    },
    [Result.Error + CheckType.Properties]: {
      color: 'red',
      variant: 'bold',
      children: 'Property',
      prefix: <Icon icon="cancel" />,
    },

    [Result.Ok + CheckType.Relationships]: {
      color: 'green',
      variant: 'bold',
      children: 'Relationship',
      prefix: <Icon icon="check_circle" />,
    },
    [Result.Error + CheckType.Relationships]: {
      color: 'orange',
      variant: 'bold',
      children: 'Relationship',
      prefix: <Icon icon="cancel" />,
    },

    [Result.Ok + CheckType.Telemetry]: {
      color: 'green',
      variant: 'outline',
      children: 'Telemetry',
      prefix: <Icon icon="check_circle" />,
    },
    [Result.Error + CheckType.Telemetry]: {
      color: 'red',
      variant: 'outline',
      children: 'Telemetry',
      prefix: <Icon icon="cancel" />,
    },

    [Result.Error + CheckType.DataQualityRule]: {
      color: 'red',
      variant: 'outline',
      children: 'Bad Rule',
      prefix: <Icon icon="cancel" />,
    },
  } as ChipPropsType;

  if (!row.resultType || !row.checkType) return <Badge color="red" children="?missingValues?" />;

  const key = row.resultType + row.checkType;
  if (!(key in statusChipPropsMap)) return <Badge color="red" children="?missingFormat?" />;

  const props = statusChipPropsMap[key];

  //@ts-ignore
  return <Badge title={props?.children} {...props} size={size} />;
};
