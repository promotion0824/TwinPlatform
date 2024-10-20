import { priorities, titleCase } from '@willow/common'
import { getPriorityTranslatedName } from '@willow/ui'
import { Badge } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'

/**
 * @deprecated
 * Please use PriorityBadge from packages/common/src/insights/component/index.tsx instead.
 */
export default function PriorityPill({ priorityId }: { priorityId: number }) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const priority = priorities.find(
    (prevPriority) => prevPriority.id === priorityId
  )

  return (
    <Badge size="md" variant="muted" color={priority?.color ?? 'gray'}>
      {titleCase({
        text: getPriorityTranslatedName(t, priority?.id ?? 0) ?? '-',
        language,
      })}
    </Badge>
  )
}
