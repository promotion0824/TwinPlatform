import { titleCase } from '@willow/common'
import { getInsightStatusTranslatedName } from '@willow/ui'
import { Badge } from '@willowinc/ui'
import insightStatuses from 'components/insightStatuses.json'
import { useTranslation } from 'react-i18next'

/**
 * legacy component to work with legacy insight status of
 * open, acknowledged, inProgress and closed
 */
export default function InsightStatusPill({ status }) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const insightStatus = insightStatuses.find(
    (insightStatusItem) => insightStatusItem.id === status
  )

  return (
    <Badge variant="dot" size="md" color={insightStatus?.color}>
      {titleCase({
        text: getInsightStatusTranslatedName(t, insightStatus?.id) ?? '-',
        language,
      })}
    </Badge>
  )
}
