import { getInsightStatusTranslatedName } from '@willow/ui'
import { titleCase } from '@willow/common'
import { InsightWorkflowStatus } from '@willow/common/insights/insights/types'
import { useTranslation } from 'react-i18next'
import { Badge, BadgeProps } from '@willowinc/ui'

/**
 * insight workflow status values of open, new, inProgress, resolved, ignored
 * figma reference: https://www.figma.com/file/dUfwhUC42QG7UkxGTgjv7Q/Insights-to-Action-V2?type=design&node-id=9452%3A40958&mode=design&t=5ZeAh4V4FA4PMQuL-1
 * confluence: https://willow.atlassian.net/wiki/spaces/MAR/pages/2387935430/Proposed+Insights+Status+Workflow
 */
export default function InsightWorkflowStatusPill({
  className,
  size = 'md',
  lastStatus,
}: {
  lastStatus: InsightWorkflowStatus
  className?: string
  size?: BadgeProps['size']
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <Badge
      className={className}
      variant="dot"
      color={insightWorkflowColorMap[lastStatus]}
      size={size}
    >
      {titleCase({
        text: getInsightStatusTranslatedName(t, lastStatus) ?? '-',
        language,
      })}
    </Badge>
  )
}

const insightWorkflowColorMap = {
  open: 'yellow',
  new: 'yellow',
  inProgress: 'blue',
  resolved: 'green',
  ignored: 'gray',
  readyToResolve: 'purple',
}
