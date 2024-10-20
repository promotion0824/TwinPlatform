import { styled } from 'twin.macro'
import AssetDetailsModal from '../../../../../components/AssetDetailsModal/AssetDetailsModal'
import Header from './Header/Header'
import AssetHistoryTable from './Table/index'
import AssetHistoryProvider, {
  FilterType,
  useAssetHistory,
} from './provider/AssetHistoryProvider'

/**
 * This component will display a list of standard tickets, scheduled tickets,
 * insights, and inspections for the given twin asset in TwinView.
 * Users will be able to filter the list by type and date range.
 * Users will also be able to select individual records from the list, where a
 * modal will popup with more information about the record.
 */
export default function AssetHistory({
  siteId,
  assetId,
  twinId,
  filterType,
  setFilterType,
  setInsightId,
}: {
  siteId: string
  assetId: string
  twinId: string
  filterType: FilterType
  setFilterType: (filterType: FilterType) => void
  setInsightId: (insightId?: string) => void
}) {
  return (
    <AssetHistoryProvider
      siteId={siteId}
      assetId={assetId}
      twinId={twinId}
      filterType={filterType}
      setFilterType={setFilterType}
      setInsightId={setInsightId}
    >
      <AssetHistoryContent />
    </AssetHistoryProvider>
  )
}

function AssetHistoryContent() {
  const {
    filterType,
    setFilterType,
    filterDateRange,
    handleDateRangeChange,
    dateRangePickerOptions,
    typeOptions,
    assetHistory,
    assetHistoryQueryStatus,
    selectedItem,
    setSelectedItem,
  } = useAssetHistory()

  return (
    <>
      <StickyContainer>
        <FilterContainer>
          <Header
            dateRangePickerOptions={dateRangePickerOptions}
            typeOptions={typeOptions}
            filterType={filterType}
            filterDateRange={filterDateRange}
            onDateRangeChange={handleDateRangeChange}
            onFilterTypeChange={setFilterType}
          />
        </FilterContainer>
      </StickyContainer>

      <div
        css={{
          width: '100%',
          height: '100%',
          overflowY: 'auto',
        }}
      >
        <AssetHistoryTable
          assetHistory={assetHistory}
          assetHistoryQueryStatus={assetHistoryQueryStatus}
          onSelectItem={setSelectedItem}
          filterType={filterType}
        />
      </div>
      {selectedItem != null && (
        <AssetDetailsModal
          siteId={selectedItem.siteId}
          item={{ ...selectedItem, modalType: selectedItem.assetHistoryType }}
          onClose={() => setSelectedItem(undefined)}
          navigationButtonProps={{
            items: assetHistory,
            selectedItem,
            setSelectedItem,
          }}
          dataSegmentPropPage={`Asset History ${getModalDataSegment(
            selectedItem.assetHistoryType
          )}`}
          times={filterDateRange}
          selectedInsightIds={[selectedItem.id]}
        />
      )}
    </>
  )
}

const getModalDataSegment = (assetHistoryType) => {
  switch (assetHistoryType) {
    case 'standardTicket':
      return 'Standard Ticket'
    case 'scheduledTicket':
      return 'Scheduled Ticket'
    default:
      return assetHistoryType
  }
}

const StickyContainer = styled.div({
  width: '100%',
  display: 'inline-table',
  position: 'sticky',
  top: 0,
  zIndex: 'var(--z-modal)',
  backgroundColor: '#252525',
})

const FilterContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'row',
  padding: `${theme.spacing.s24} ${theme.spacing.s16} ${theme.spacing.s16}`,
  justifyContent: 'space-between',
}))
