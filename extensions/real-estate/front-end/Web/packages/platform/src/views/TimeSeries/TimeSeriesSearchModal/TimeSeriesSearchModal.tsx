import { useEffect } from 'react'
import { styled } from 'twin.macro'
import { Modal } from '@willowinc/ui'

import TimeSeriesSearchResultsAddRemoveButton from './TimeSeriesSearchResultsAddRemoveButton'

import SearchResultsProvider, {
  useSearchResults,
} from '../../Portfolio/twins/results/page/state/SearchResults'
import SearchResultsPanels from '../../Portfolio/twins/results/page/ui/SearchResultsPanels'

const StyledModal = styled(Modal)({
  '[role="dialog"]': {
    height: '100%',
  },
})

const StyledSearchResultsPanels = styled(SearchResultsPanels)(({ theme }) => ({
  padding: theme.spacing.s4,
  backgroundColor: theme.color.neutral.bg.base.default,
  height: 'initial !important',
}))

function TimeSeriesSearchResults() {
  const { setDisableCognitiveSearch, setHasFileSearch } = useSearchResults()

  useEffect(() => {
    setDisableCognitiveSearch(true)
    setHasFileSearch(false)
  }, [])

  return (
    <StyledSearchResultsPanels
      disableResultLinks
      hideHeaderControls
      ResultsListButton={TimeSeriesSearchResultsAddRemoveButton}
      typeaheadZIndex="203" // To put the typeahead above the modal's z-index (201)
      gapSize="small"
    />
  )
}

export default function TimeSeriesSearchModal({
  onClose,
  opened,
}: {
  onClose: () => void
  opened: boolean
}) {
  return (
    <StyledModal
      header="Search"
      opened={opened}
      onClose={onClose}
      scrollInBody
      size="90%"
    >
      <SearchResultsProvider>
        <TimeSeriesSearchResults />
      </SearchResultsProvider>
    </StyledModal>
  )
}
