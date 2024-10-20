import tw from 'twin.macro'
import { Modal } from '@willow/ui'
import { ModalType } from '../AssetDetailsModal'
import DeleteInsightsConfirmation from './DeleteInsightsConfirmation'
import ResolveInsightConfirmation from './ResolveInsightConfirmation'

/**
 * A small size rectangular shaped modal in the mid of screen asking user to confirm an action,
 * it could be to resolve an insight, or delete single or many insights.
 * component is expected to be used inside "InsightsProvider"
 * figma reference: https://www.figma.com/file/dUfwhUC42QG7UkxGTgjv7Q/Insights-to-Action-V2?type=design&node-id=4568-76924&t=seuW9xlu0v5Qzicc-0
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/78758
 */
export default function ConfirmationModal({
  selectedInsightIds = [],
  siteId,
  onClose,
  modalType,
  onActionChange,
  onClearInsightIds,
}: {
  siteId?: string
  onClose: () => void
  selectedInsightIds?: string[]
  modalType: ModalType
  onActionChange?: (action?: string) => void
  onClearInsightIds?: () => void
}) {
  return (
    <Modal isFormHeader size="full" onClose={onClose}>
      <div tw="w-full h-full flex items-center justify-center">
        {modalType === 'deleteInsightsConfirmation' ? (
          <DeleteInsightsConfirmation
            onClose={onClose}
            siteId={siteId}
            selectedInsightIds={selectedInsightIds}
            onClearInsightIds={onClearInsightIds}
          />
        ) : modalType === 'resolveInsightConfirmation' ? (
          <ResolveInsightConfirmation
            onActionChange={onActionChange}
            onClose={onClose}
          />
        ) : null}
      </div>
    </Modal>
  )
}
