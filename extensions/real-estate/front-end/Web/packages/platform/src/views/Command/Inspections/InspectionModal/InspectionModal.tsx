import { useTranslation } from 'react-i18next'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import tw from 'twin.macro'
import { Fetch, Modal } from '@willow/ui'
import InspectionForm from './InspectionForm'

/**
 * Modal to view an inspection (existing and new) from Inspections page and Zones page.
 */
export default function InspectionModal({
  zoneId,
  inspection,
  readOnly = false,
  onClose,
}: {
  /**
   * The zoneId in view - Provided when we are viewing inspection from zones page.
   */
  zoneId?: string
  /**
   * The inspection object. Id is optional for new inspection item.
   */
  inspection: { siteId: string; id?: string }
  readOnly: boolean
  onClose: () => void
}) {
  const { t } = useTranslation()

  return (
    <Modal
      closeOnClickOutside={false}
      header={
        inspection?.id == null
          ? t('plainText.newInspection')
          : t('plainText.inspection')
      }
      onClose={onClose}
      tw="w-[810px]"
    >
      <Fetch
        url={
          inspection?.id != null
            ? `/api/sites/${inspection.siteId}/inspections/${inspection.id}`
            : undefined
        }
      >
        {(response) => (
          <InspectionForm
            inspection={{
              siteId: inspection.siteId,
              zoneId,
              floorCode: null,
              assetId: null,
              assetName: '',
              name: '',
              assets: [],
              assignedWorkgroupId: null,
              assignedWorkgroupName: '',
              startDate: null, // ??
              endDate: null, // ??
              frequency: 8,
              frequencyUnit: 'hours',
              ...response,
              checks:
                response?.checks.map((check) => ({
                  ...check,
                  dependencyName:
                    response.checks.find(
                      (dependencyCheck) =>
                        dependencyCheck.id === check.dependencyId
                    )?.name ?? '',
                })) ?? [],
            }}
            readOnly={readOnly}
          />
        )}
      </Fetch>
    </Modal>
  )
}
