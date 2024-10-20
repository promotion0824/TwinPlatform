import { useTranslation } from 'react-i18next'
import { KpiSummarySettings } from '../../../../../store/buildingHomeSlice'
import EditWidgetModal from '../EditWidgetModal'
import KPISummaryWidgetEditForm from './KPISummaryWidgetEditForm'

interface KPISummaryWidgetEditModalProps {
  onClose: () => void
  onCancel: () => void
  onSave: () => void
  opened: boolean
  options: KpiSummarySettings
  onSaveOptions: (newOptions: KpiSummarySettings) => void
}

const KPISummaryWidgetEditModal = ({
  onClose,
  onCancel,
  onSave,
  opened,
  options,
  onSaveOptions,
}: KPISummaryWidgetEditModalProps) => {
  const { t } = useTranslation()

  return (
    <EditWidgetModal
      header={t('headers.editKpiSummary')}
      onCancel={onCancel}
      onClose={onClose}
      onSave={onSave}
      opened={opened}
    >
      <KPISummaryWidgetEditForm
        options={options}
        onSaveOptions={onSaveOptions}
      />
    </EditWidgetModal>
  )
}

export default KPISummaryWidgetEditModal
