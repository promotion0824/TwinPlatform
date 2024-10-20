import { Badge } from '@willowinc/ui';
import { Status } from '../../../services/Clients';

export default function StatusBadge({ status }: { status: Status }) {
  const size = 'sm';

  const statusPropsMap = {
    [Status.Pending]: { color: 'blue', variant: 'subtle', size },
    [Status.Approved]: { color: 'green', variant: 'subtle', size },
    [Status.Ignore]: { color: 'gray', variant: 'subtle', size },
    [Status.Created]: { color: 'purple', variant: 'subtle', size },
  } as Record<
    Status,
    {
      color: 'gray' | 'red' | 'pink' | 'blue' | 'cyan' | 'green' | 'yellow' | 'orange' | 'teal' | 'purple';
      variant: 'bold' | 'muted' | 'outline' | 'subtle' | 'dot';
      children: React.ReactNode;
      size: 'xs' | 'sm' | 'md' | 'lg';
    }
  >;

  const props = statusPropsMap[status] || { color: 'gray', variant: 'outline', size };

  return (
    //@ts-ignore
    <Badge title={status} {...props}>
      {status}
    </Badge>
  );
}
