import { titleCase } from '@willow/common'
import { Switch } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import type { InsightsSettings } from '../../../../../store/buildingHomeSlice/defaultWidgetConfig'
import EditWidgetModal from '../EditWidgetModal'

const Subheading = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

const InsightsWidgetEditModal = ({
  onCancel,
  onClose,
  onSave,
  opened,
  setWidgetConfig,
  widgetConfig,
}: {
  onCancel: () => void
  onClose: () => void
  onSave: () => void
  opened: boolean
  setWidgetConfig: (config: InsightsSettings) => void
  widgetConfig: InsightsSettings
}) => {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const configurationKeys = [
    'showActiveAvoidableCost',
    'showActiveAvoidableEnergy',
    'showAverageDuration',
  ]

  return (
    <EditWidgetModal
      header={titleCase({ language, text: t('headers.editInsights') })}
      onCancel={onCancel}
      onClose={onClose}
      onSave={onSave}
      opened={opened}
    >
      <Subheading>{t('headers.configuration')}</Subheading>
      {configurationKeys.map((key) => (
        <Switch
          checked={widgetConfig[key]}
          justify="space-between"
          label={titleCase({
            language,
            text: t(`labels.${key}`),
          })}
          labelPosition="left"
          onChange={(event) =>
            setWidgetConfig({
              ...widgetConfig,
              [key]: event.currentTarget.checked,
            })
          }
        />
      ))}
    </EditWidgetModal>
  )
}

export default InsightsWidgetEditModal
