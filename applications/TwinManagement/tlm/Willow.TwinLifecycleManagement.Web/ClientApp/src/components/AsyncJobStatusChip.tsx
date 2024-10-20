import { AsyncJobStatus } from '../services/Clients';
import { Badge } from '@willowinc/ui';

export const AsyncJobStatusChip = ({ value, className }: { value: AsyncJobStatus; className?: string }) => {
  const children = value;
  const size = 'sm';

  const statusChipPropsMap = {
    [AsyncJobStatus.Queued]: { color: 'gray', variant: 'outline', size, children },
    [AsyncJobStatus.Processing]: { color: 'gray', variant: 'bold', size, children },
    [AsyncJobStatus.Done]: { color: 'green', variant: 'bold', size, children },
    [AsyncJobStatus.Error]: { color: 'red', variant: 'bold', size, children },
    [AsyncJobStatus.Canceled]: { color: 'yellow', variant: 'bold', size, children },
    [AsyncJobStatus.CancelPending]: { color: 'gray', variant: 'dot', size, children },
    [AsyncJobStatus.DeletePending]: { color: 'gray', variant: 'dot', size, children },
    [AsyncJobStatus.Aborted]: { color: 'gray', variant: 'dot', size, children },
  } as Record<
    AsyncJobStatus,
    {
      color: 'gray' | 'red' | 'pink' | 'blue' | 'cyan' | 'green' | 'yellow' | 'orange' | 'teal' | 'purple';
      variant: 'bold' | 'muted' | 'outline' | 'subtle' | 'dot';
      children: React.ReactNode;
      size: 'xs' | 'sm' | 'md' | 'lg';
    }
  >;

  const props = statusChipPropsMap[value];
  //@ts-ignore
  return <Badge title={value} {...props} className={className} />;
};
