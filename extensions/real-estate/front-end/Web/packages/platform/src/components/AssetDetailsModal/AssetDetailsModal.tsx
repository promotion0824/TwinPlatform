/* eslint-disable complexity */
import {
  Insight,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import { useState } from 'react'
import { useQueryClient } from 'react-query'
import ContactUsForm from '../../views/Layout/Layout/Header/ContactUs/ContactUsForm'
import { AssetHistory } from '../../views/Portfolio/twins/view/AssetHistory/hooks/useGetAssetHistory'
import ConfirmationModal from './ConfirmationModal/ConfirmationModal'
import InsightWorkflowModal from './InsightModal/InsightWorkflowModal'
import InspectionModal from './InspectionModal/InspectionModal'
import NewTicketModal from './TicketModal/NewTicketModal'
import TicketModal from './TicketModal/TicketModal'

export type ModalType =
  | 'insight'
  | 'inspection'
  | 'ticket'
  | 'standardTicket'
  | 'scheduledTicket'
  | 'newTicket'
  | 'deleteInsightsConfirmation' // delete single or multiple insights
  | 'resolveInsightConfirmation' // resolve single insight
  | 'report' // report single or multiple insights

/**
 * The Insight or Ticket or Inspection item
 */
export type Item = {
  modalType: ModalType
  id?: string
  selectedInsight?: {
    id: Insight['id']
    name: Insight['name']
    ruleName: Insight['ruleName']
  }
  [key: string]: unknown
}

const findSelectedItemIndex = (items, selectedItem) =>
  items.findIndex(
    (item) =>
      item.id === selectedItem?.id &&
      (!selectedItem.assetHistoryType ||
        item.assetHistoryType === selectedItem?.assetHistoryType)
  )

/**
 * This modal displays the {@see InsightModal}, or {@see TicketModal}, or {@see InspectionModal}
 * based on the type specified.
 *
 * Note: An insight may be linked to multiple tickets, and a ticket may be
 * linked to an insight. When the user clicks a linked ticket or insight, this
 * component manages the view update.
 */
export default function AssetDetailsModal({
  siteId,
  item,
  onClose,
  navigationButtonProps,
  dataSegmentPropPage,
  times,
  isUpdatedTicket = false,
  onClearSelectedInsightIds = () => {},
  insightTab,
  onInsightTabChange,
  canDeleteInsight = false,
  controlledCurrentItem,
  onControlledCurrentItemChange,
  onActionChange,
  selectedInsightIds = [],
}: {
  siteId: string
  item: Item
  onClose: () => void
  /**
   * If present, shows the prev/next button to view the next item in this modal on the list.
   */
  navigationButtonProps: {
    items: any
    selectedItem: any
    setSelectedItem: (selected: AssetHistory | Insight | undefined) => void
  }
  isUpdatedTicket?: boolean
  dataSegmentPropPage: string
  /**
   * Date-time from and until, mainly used for "inspection" modal.
   */
  times?: [string, string]
  /**
   * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/75605
   * To not have InsightForm (grandchild of this component) dependent on InsightsProvider
   */
  onClearSelectedInsightIds?: () => void
  insightTab?: string
  onInsightTabChange?: (tab?: string) => void
  canDeleteInsight?: boolean
  /**
   * allow parent component to control "currentItem" will enable us to
   * say open a ticket modal when insight modal is closed
   */
  controlledCurrentItem?: Item
  onControlledCurrentItemChange?: (item?: Item) => void
  /**
   * to be used by confirmation modal when resolving insight,
   * by calling this function, we can achieve the effect that
   * closure of ticket model will open confirmation modal
   */
  onActionChange?: (action?: string) => void
  selectedInsightIds?: string[]
}) {
  const queryClient = useQueryClient()
  /**
   * This state is used to show 'Submit ticket' when creating a new ticket
   * and 'Save ticket' when updating an existing ticket
   */
  const [isTicketUpdated, setIsTicketUpdated] = useState(
    isUpdatedTicket || false
  )

  // State to store the metadata of the current viewing item (insight/ticket/inspection),
  // needed to keep track of the modal view in the case when linked item is viewed.
  // See handleClose below.
  const [uncontrolledCurrentItem, setUncontrolledCurrentItem] =
    useState<Item>(item)
  const { items, selectedItem, setSelectedItem } = navigationButtonProps || {}

  const currentItem = onControlledCurrentItemChange
    ? controlledCurrentItem
    : uncontrolledCurrentItem
  const setCurrentItem =
    onControlledCurrentItemChange ?? setUncontrolledCurrentItem
  /**
   * Close the modal, except when there is a nextModalView. In this case, we
   * update the current view to be displayed in this modal. This happens in the
   * following scenarios:
   * - Clicked on linked item like linked ticket from InsightModal or linked
   *   insight from TicketModal.
   * - Creation of new ticket from Insight.
   */
  function handleClose(nextItem?: Item) {
    if (nextItem?.modalType != null) {
      setCurrentItem(nextItem)
    } else {
      onInsightTabChange?.()
      onClose()
    }
  }

  const handleNavigateItem = (delta = 1) => {
    const currentSelectedItemIndex = findSelectedItemIndex(items, selectedItem)
    const nextSelectedItemIndex = currentSelectedItemIndex + delta

    if (0 <= nextSelectedItemIndex && nextSelectedItemIndex < items.length) {
      const nextItem = items[nextSelectedItemIndex]
      setSelectedItem(nextItem)
      setCurrentItem({
        ...nextItem,
        modalType: nextItem.assetHistoryType || item.modalType,
      })
    }
  }

  function onReportInsight() {
    // refresh insights tables and asset insights table
    queryClient.invalidateQueries(['insights'])
    queryClient.invalidateQueries(['asset-insights'])
    queryClient.invalidateQueries(['insightInfo', siteId, currentItem?.id])
    onClearSelectedInsightIds()
  }

  // Whether to show button to navigate up and down the list, when the currentView is not
  // a new item (id is undefined), and is indeed the selected item (i.e. not currently
  // viewing the linked item of selectedItem)
  const isNavigationButtonShown =
    navigationButtonProps != null &&
    currentItem?.id != null &&
    selectedItem?.id === currentItem?.id

  switch (currentItem?.modalType) {
    case 'insight':
      return (
        <InsightWorkflowModal
          siteId={siteId}
          insightId={currentItem?.id as string}
          // show insight's ruleName as its name if it has one just like in the insight table
          name={currentItem?.ruleName as string | undefined}
          lastStatus={currentItem.lastStatus as InsightWorkflowStatus} // guaranteed to be an Insight where lastStatus is for sure defined
          onClose={handleClose}
          showNavigationButtons={isNavigationButtonShown}
          onPreviousItem={() => handleNavigateItem(-1)}
          onNextItem={() => handleNavigateItem(1)}
          setIsTicketUpdated={setIsTicketUpdated}
          insightTab={insightTab}
          onInsightTabChange={onInsightTabChange}
          canDeleteInsight={canDeleteInsight}
        />
      )
    case 'newTicket':
      return (
        <NewTicketModal
          siteId={siteId}
          ticket={currentItem}
          insightId={currentItem.insightId}
          insightName={currentItem.insightName}
          onClose={handleClose}
          dataSegmentPropPage={dataSegmentPropPage}
        />
      )
    case 'ticket': // falls through
    case 'standardTicket': // falls through
    case 'scheduledTicket':
      return (
        <TicketModal
          siteId={siteId}
          ticketId={currentItem?.id as string}
          onClose={handleClose}
          ticket={currentItem}
          showNavigationButtons={isNavigationButtonShown}
          onPreviousItem={() => handleNavigateItem(-1)}
          onNextItem={() => handleNavigateItem(1)}
          dataSegmentPropPage={dataSegmentPropPage}
          isTicketUpdated={isTicketUpdated}
          selectedInsight={currentItem.selectedInsight}
        />
      )
    case 'inspection':
      return (
        <InspectionModal
          siteId={siteId}
          inspectionId={currentItem.id}
          onClose={handleClose}
          showNavigationButtons={isNavigationButtonShown}
          onPreviousItem={() => handleNavigateItem(-1)}
          onNextItem={() => handleNavigateItem(1)}
          times={times}
        />
      )
    case 'deleteInsightsConfirmation':
    case 'resolveInsightConfirmation':
      return (
        <ConfirmationModal
          siteId={siteId}
          selectedInsightIds={selectedInsightIds}
          onClearInsightIds={onClearSelectedInsightIds}
          onClose={handleClose}
          modalType={currentItem.modalType}
          onActionChange={onActionChange}
        />
      )
    case 'report':
      return (
        <ContactUsForm
          siteId={siteId}
          insightIds={selectedInsightIds}
          onClearInsightIds={onClearSelectedInsightIds}
          onSubmitForm={onReportInsight}
          isFormOpen
          onClose={onClose}
        />
      )
    default:
      return null
  }
}
