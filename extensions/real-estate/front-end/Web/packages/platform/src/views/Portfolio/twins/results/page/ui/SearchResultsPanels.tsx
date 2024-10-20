import { styled } from 'twin.macro'
import { titleCase } from '@willow/common'
import { useLanguage } from '@willow/ui'
import {
  Badge,
  Button,
  Panel,
  PanelContent,
  PanelGroup,
  SegmentedControl,
} from '@willowinc/ui'

import InjectedResults from './Results/Results'
import InjectedSearch from './Search/Search'
import { useSearchResults as useSearchResultsInjected } from '../state/SearchResults'

const StyledPanelContent = styled(PanelContent)({
  height: '100%',
})

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

const SearchResultsHeader = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s8,
}))

export default function SearchResultsPanels({
  className = undefined,
  disableResultLinks = false,
  hideHeaderControls = false,
  Results = InjectedResults,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  ResultsListButton = ({ twin }) => <></>,
  Search = InjectedSearch,
  showFiltersPanel = true,
  typeaheadZIndex = 'var(--z-dropdown)',
  useSearchResults = useSearchResultsInjected,
  ...rest
}) {
  const { language } = useLanguage()
  const {
    t,
    display,
    changeDisplay,
    resetFilters,
    tableDisplayIsDisabled,
    twins,
  } = useSearchResults()

  return (
    <StyledPanelGroup className={className} {...rest}>
      {showFiltersPanel ? (
        <Panel
          collapsible
          defaultSize={320}
          footer={
            <Button
              background="transparent"
              kind="secondary"
              onClick={() => resetFilters()}
            >
              {titleCase({ text: t('labels.resetFilters'), language })}
            </Button>
          }
          title={t('headers.filters')}
        >
          <StyledPanelContent>
            <Search typeaheadZIndex={typeaheadZIndex} />
          </StyledPanelContent>
        </Panel>
      ) : (
        <></>
      )}
      <Panel
        headerControls={
          !hideHeaderControls &&
          !tableDisplayIsDisabled && (
            <SegmentedControl
              data={[
                {
                  iconName: 'view_list',
                  iconOnly: true,
                  label: 'List',
                  value: 'list',
                },
                {
                  iconName: 'view_column',
                  iconOnly: true,
                  label: 'Table',
                  value: 'table',
                },
              ]}
              onChange={changeDisplay}
              value={display}
            />
          )
        }
        title={
          <SearchResultsHeader>
            <div>{t('headers.results')}</div>
            {twins && <Badge>{twins.length > 99 ? '99+' : twins.length}</Badge>}
          </SearchResultsHeader>
        }
      >
        <StyledPanelContent tw="h-full">
          <Results
            disableLinks={disableResultLinks}
            ResultsListButton={ResultsListButton}
          />
        </StyledPanelContent>
      </Panel>
    </StyledPanelGroup>
  )
}
