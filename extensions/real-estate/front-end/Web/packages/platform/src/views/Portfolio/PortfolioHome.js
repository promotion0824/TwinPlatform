import { ResizeObserverContainer, titleCase } from '@willow/common'
import {
  FILTER_PANEL_BREAKPOINT,
  DocumentTitle,
  useFeatureFlag,
  useScopeSelector,
} from '@willow/ui'
import {
  Badge,
  Button,
  IconButton,
  Indicator,
  Menu,
  Panel,
  PanelContent,
  PanelGroup,
  Radio,
  RadioGroup,
} from '@willowinc/ui'
import { useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'
import Filters from './Filters/Filters'
import Map from './Map/Map'
import { usePortfolio } from './PortfolioContext'
import PortfolioHomeHeader from './PortfolioHomeHeader'
import usePagedPortfolio from './Sites/PagedPortfolioContext'
import Sites from './Sites/Sites'

const PortfolioHome = () => {
  const featureFlags = useFeatureFlag()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const mapContainerRef = useRef(null)
  const { resetFilters, filteredSiteIds } = usePortfolio()
  const { enabled: pagedPortfolioEnabled, numOfAllSitesCanBeLoaded } =
    usePagedPortfolio()
  const { locationName } = useScopeSelector()

  const [currentPageWidth, setCurrentPageWidth] = useState(Infinity)
  const [sortingOption, setSortingOption] = useState('alphabetical')
  const showFiltersPanel = currentPageWidth > FILTER_PANEL_BREAKPOINT

  const sortingOptions = [
    {
      label: titleCase({ language, text: t('plainText.alphabetical') }),
      value: 'alphabetical',
    },
    {
      label: t('plainText.performanceKpiHighest'),
      value: 'performanceKpiHighest',
    },
    {
      label: t('plainText.performanceKpiLowest'),
      value: 'performanceKpiLowest',
    },
    ...(featureFlags.hasFeatureToggle('locationCardInsightsByPriority')
      ? [
          {
            label: titleCase({
              language,
              text: t('plainText.noOfCriticalInsights'),
            }),
            value: 'noOfCriticalInsights',
          },
        ]
      : []),
    {
      label: titleCase({ language, text: t('plainText.noOfTotalInsights') }),
      value: 'noOfTotalInsights',
    },
    {
      label: titleCase({ language, text: t('plainText.noOfTickets') }),
      value: 'noOfTickets',
    },
  ]

  return (
    <ResizeObserverContainer
      onContainerWidthChange={setCurrentPageWidth}
      style={{ height: '100%' }}
    >
      <DocumentTitle scopes={[t('headers.home'), locationName]} />

      <ContentContainer>
        <PortfolioHomeHeader
          key={showFiltersPanel.toString()}
          showFilters={!showFiltersPanel}
        />

        <PanelGroup
          gapSize="medium"
          css={css(({ theme }) => ({
            padding: theme.spacing.s16,
          }))}
        >
          {showFiltersPanel ? (
            <Panel
              title={t('headers.filters')}
              defaultSize={240}
              collapsible
              footer={
                <Button
                  kind="secondary"
                  css={{
                    textTransform: 'capitalize',
                  }}
                  onClick={resetFilters}
                >
                  {t('labels.resetFilters')}
                </Button>
              }
            >
              <PanelContent css={{ height: '100%' }}>
                {/* TODO: until data team fix reports so they can work with the site ids
             we send in, we need to hide Filters; Filters help on generate site ids,
             and current Portfolio Sigma Reports ignores them */}
                <Filters />
              </PanelContent>
            </Panel>
          ) : (
            <></>
          )}
          <PanelGroup gapSize="medium" resizable>
            <Panel
              collapsible
              headerControls={
                <Menu>
                  <Menu.Target>
                    <Indicator
                      role="button"
                      disabled={sortingOption === 'alphabetical'}
                    >
                      <IconButton
                        background="transparent"
                        icon="sort"
                        kind="secondary"
                      />
                    </Indicator>
                  </Menu.Target>
                  <Menu.Dropdown>
                    <Menu.Label>{t('plainText.sortOrder')}</Menu.Label>
                    <RadioGroup
                      onChange={setSortingOption}
                      value={sortingOption}
                    >
                      {sortingOptions.map((option) => (
                        <Menu.Item key={option.label} closeMenuOnClick={false}>
                          <Radio label={option.label} value={option.value} />
                        </Menu.Item>
                      ))}
                    </RadioGroup>
                  </Menu.Dropdown>
                </Menu>
              }
              title={
                <LocationHeader
                  numOfLocations={
                    pagedPortfolioEnabled
                      ? numOfAllSitesCanBeLoaded
                      : filteredSiteIds.length
                  }
                />
              }
            >
              <PanelContent css={{ height: '100%' }}>
                <Sites sortingOption={sortingOption} />
              </PanelContent>
            </Panel>
            <Panel title={t('plainText.map')} collapsible>
              <PanelContent css={{ height: '100%' }} ref={mapContainerRef}>
                <Map containerRef={mapContainerRef} />
              </PanelContent>
            </Panel>
          </PanelGroup>
        </PanelGroup>
      </ContentContainer>
    </ResizeObserverContainer>
  )
}

const LocationHeader = ({ numOfLocations }) => {
  const { t } = useTranslation()

  return (
    <LocationHeaderContainer>
      {t('headers.locations')}
      <Badge>{numOfLocations}</Badge>
    </LocationHeaderContainer>
  )
}

const ContentContainer = styled.div`
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
`

const LocationHeaderContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    gap: ${theme.spacing.s8};
    align-items: center;
    text-transform: capitalize;
  `
)

export default PortfolioHome
