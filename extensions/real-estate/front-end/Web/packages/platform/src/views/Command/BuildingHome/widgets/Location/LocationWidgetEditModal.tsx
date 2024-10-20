import { titleCase } from '@willow/common'
import { Switch } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { styled } from 'styled-components'
import EditWidgetModal from '../EditWidgetModal'

const Subheading = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

const LocationWidgetEditModal = ({
  onClose,
  onCancel,
  onSave,
  opened,

  showOverallPerformance,
  onShowOverallPerformanceChange,
}: {
  onClose: () => void
  onCancel: () => void
  onSave: () => void
  opened: boolean

  showOverallPerformance: boolean
  onShowOverallPerformanceChange: (value: boolean) => void
}) => {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  return (
    <EditWidgetModal
      header={titleCase({ language, text: t('headers.editLocationSummary') })}
      onCancel={onCancel}
      onClose={onClose}
      onSave={onSave}
      opened={opened}
    >
      <Subheading>{t('headers.configuration')}</Subheading>

      <Switch
        checked={showOverallPerformance}
        justify="space-between"
        label={titleCase({
          language,
          text: t('labels.showOverallPerformance'),
        })}
        labelPosition="left"
        onChange={(event) =>
          onShowOverallPerformanceChange(event.currentTarget.checked)
        }
      />
    </EditWidgetModal>
  )
}

export default LocationWidgetEditModal
