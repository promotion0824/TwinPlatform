import { titleCase } from '@willow/common'
import { SyncStatus } from '@willow/common/ticketStatus'
import { Badge, Icon, IconName } from '@willowinc/ui'
import { Colors } from '@willowinc/ui/src/lib/common'
import { useTranslation } from 'react-i18next'

const SyncStatusComponent = ({ syncStatus }: { syncStatus?: SyncStatus }) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const syncStatusMap: {
    [key in SyncStatus]: {
      icon: IconName
      color: Colors
      badgeLabel: string
    }
  } = {
    [SyncStatus.Failed]: {
      icon: 'close',
      color: 'red',
      badgeLabel: 'plainText.syncFailed',
    },
    [SyncStatus.InProgress]: {
      icon: 'sync',
      color: 'gray',
      badgeLabel: 'plainText.syncInProgress',
    },
    [SyncStatus.Completed]: {
      icon: 'check',
      color: 'pink',
      badgeLabel: 'plainText.syncComplete',
    },
  }

  if (!syncStatus) {
    return null
  }

  return (
    <Badge
      size="sm"
      prefix={<Icon icon={syncStatusMap[syncStatus].icon} />}
      color={syncStatusMap[syncStatus].color}
    >
      {titleCase({
        text: t(syncStatusMap[syncStatus].badgeLabel),
        language,
      })}
    </Badge>
  )
}

export default SyncStatusComponent
