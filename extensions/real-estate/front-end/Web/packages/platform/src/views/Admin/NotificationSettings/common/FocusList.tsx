import { Skill, Twin, titleCase } from '@willow/common'
import { InsightTypeBadge } from '@willow/common/insights/component'
import {
  InsightType,
  insightTypes,
} from '@willow/common/insights/insights/types'
import { getModelInfo } from '@willow/common/twins/utils'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { TooltipWhenTruncated, TwinChip } from '@willow/ui'
import { Button, Group, Icon } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import useOntologyInPlatform from '../../../../hooks/useOntologyInPlatform'

const isTwin = (item: Skill | Twin | string): item is Twin =>
  typeof item === 'object' && 'modelId' in item

const isTwinCategoryId = (item: Skill | Twin | string): item is string =>
  typeof item === 'string' || item == null

const isSkillCategory = (item: Skill | Twin | string): item is InsightType =>
  typeof item === 'string' && insightTypes.includes(_.camelCase(item))

const FocusList = ({
  focus,
  onRemove,
  disabled = false,
}: {
  focus: Skill | Twin | string
  onRemove: (item: Skill | Twin | string) => void
  disabled?: boolean
}) => {
  const translation = useTranslation()
  const {
    t,
    i18n: { language },
  } = translation

  const { data: { items: modelsOfInterest } = {} } = useModelsOfInterest()
  const { data: ontology } = useOntologyInPlatform()

  const model =
    isTwin(focus) && focus.modelId
      ? ontology?.getModelById(focus.modelId)
      : isTwinCategoryId(focus) && focus
      ? ontology?.getModelById(focus)
      : undefined

  const modelInfo =
    model && ontology && modelsOfInterest
      ? getModelInfo(model, ontology, modelsOfInterest, translation)
      : undefined

  const focusName = isTwinCategoryId(focus)
    ? modelInfo?.displayName ??
      titleCase({ text: t('plainText.allCategories'), language })
    : titleCase({ text: focus?.name, language })

  return (
    <Group w="100%" mx="20px" tw="justify-between">
      <TooltipWhenTruncated label={focusName} w="100%">
        {isTwin(focus) ? (
          <TwinChip
            variant="instance"
            modelOfInterest={modelInfo?.modelOfInterest}
            text={focus.name}
            highlightOnHover
          />
        ) : isSkillCategory(focus) ? (
          <InsightTypeBadge type={_.camelCase(focus)} badgeSize="md" />
        ) : (
          <ListText $fullWidth={false}>{focusName}</ListText>
        )}
      </TooltipWhenTruncated>
      <Button
        prefix={<Icon icon="remove" />}
        kind="secondary"
        disabled={disabled}
        onClick={() => onRemove(focus)}
      >
        {titleCase({ text: t('plainText.remove'), language })}
      </Button>
    </Group>
  )
}

export const ListText = styled.div<{ $fullWidth: boolean }>(
  ({ theme, $fullWidth }) => ({
    ...theme.font.heading.sm,
    color: theme.color.neutral.fg.muted,
    width: $fullWidth ? '500px' : '300px',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  })
)

export default FocusList
