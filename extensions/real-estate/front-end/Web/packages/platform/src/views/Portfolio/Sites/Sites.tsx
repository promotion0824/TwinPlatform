import {
  VirtualItem,
  Virtualizer,
  useVirtualizer,
} from '@tanstack/react-virtual'
import { upperCase } from 'lodash'
import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'

import { FullSizeLoader, PagedSiteResult } from '@willow/common'
import { Site } from '@willow/common/site/site/types'
import {
  NotFound,
  Progress,
  useIntersectionObserverRef,
  useScopeSelector,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import LocationCard, { ContainmentWrapper } from '../LocationCard/LocationCard'
import { usePortfolio } from '../PortfolioContext'
import { SiteSortingOption } from '../types'
import usePagedPortfolio from './PagedPortfolioContext'

export default function Sites({
  sortingOption,
}: {
  sortingOption: SiteSortingOption
}) {
  const parentRef = useRef<HTMLDivElement>(null)
  const { twinQuery } = useScopeSelector()
  const {
    enabled: paginationEnabled,
    pagedSites,
    queryResult: { hasNextPage, isFetchingNextPage, isLoading, fetchNextPage },
  } = usePagedPortfolio()

  const {
    baseMapSites,
    buildingScores,
    filteredSites,
    selectedSite,
    handleResetMapClick,
  } = usePortfolio()

  const { t } = useTranslation()

  const locationCardRefs = useRef<Record<string, HTMLDivElement | null>>({})

  useEffect(() => {
    locationCardRefs.current[selectedSite?.id]?.scrollIntoView({
      behavior: 'smooth',
    })
  }, [selectedSite?.id])

  const scoresByLocation = {}

  buildingScores.forEach((score) => {
    if (
      !score.siteId ||
      (!score.comfort && !score.energy && !score.performance)
    )
      return
    if (!scoresByLocation[score.siteId]) {
      scoresByLocation[score.siteId] = {
        comfort:
          typeof score.comfort === 'number'
            ? Math.round(score.comfort * 100)
            : null,
        energy:
          typeof score.energy === 'number'
            ? Math.round(score.energy * 100)
            : null,
        performance: Math.floor((score.performance ?? 0) * 100),
      }
    }
  })

  const getTotalInsightsCount = (site: Site) =>
    (site.insightsStatsByStatus?.inProgressCount ?? 0) +
    (site.insightsStatsByStatus?.newCount ?? 0) +
    (site.insightsStatsByStatus?.openCount ?? 0)

  const getTotalTicketsCount = (site: Site) =>
    site.ticketStatsByStatus?.openCount ?? 0

  const sortedSites = filteredSites.sort((a, b) => {
    if (sortingOption === 'alphabetical') {
      return a.name.localeCompare(b.name)
    }

    if (sortingOption === 'noOfCriticalInsights') {
      return b.insightsStats.urgentCount - a.insightsStats.urgentCount
    }

    if (sortingOption === 'noOfTotalInsights') {
      return getTotalInsightsCount(b) - getTotalInsightsCount(a)
    }

    if (sortingOption === 'noOfTickets') {
      return getTotalTicketsCount(b) - getTotalTicketsCount(a)
    }

    if (sortingOption === 'performanceKpiHighest') {
      return (
        (scoresByLocation[b.id]?.performance ?? Number.NEGATIVE_INFINITY) -
        (scoresByLocation[a.id]?.performance ?? Number.NEGATIVE_INFINITY)
      )
    }

    if (sortingOption === 'performanceKpiLowest') {
      return (
        (scoresByLocation[a.id]?.performance ?? Number.POSITIVE_INFINITY) -
        (scoresByLocation[b.id]?.performance ?? Number.POSITIVE_INFINITY)
      )
    }

    return a.name.localeCompare(b.name)
  })

  const gap = 12
  const virtualizer = useVirtualizer({
    count: sortedSites.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 300 /* recommend to estimate the largest possible */,
    overscan: 10,
    getItemKey: (index) => sortedSites[index].id,
    gap /* required if you have gap between listed items */,
  })

  return (
    <>
      {paginationEnabled ? (
        isLoading ? (
          <Progress />
        ) : (
          <ContainmentWrapper>
            {pagedSites && pagedSites.length > 0 && (
              <SitesContainer>
                {pagedSites.map((site: PagedSiteResult) => (
                  <LocationCard
                    data-testid={`site-${site.id}`}
                    isSelected={site.id === selectedSite?.id}
                    key={site.id}
                    scores={scoresByLocation[site.id]}
                    site={site}
                    ref={(ref) => {
                      locationCardRefs.current[site.id] = ref
                    }}
                  />
                ))}
                {isFetchingNextPage ? (
                  <Progress />
                ) : hasNextPage ? (
                  <FetchNextPage
                    onView={fetchNextPage}
                    dependencies={pagedSites}
                  />
                ) : null}
              </SitesContainer>
            )}

            {/* map feature needs further discussion, and will be added back later */}
          </ContainmentWrapper>
        )
      ) : twinQuery.status === 'loading' ? (
        <FullSizeLoader intent="secondary" />
      ) : (
        <ContainmentWrapper>
          <VirtualScrollableContainer ref={parentRef} tabIndex={0}>
            <ItemsContainer $virtualizer={virtualizer}>
              {virtualizer.getVirtualItems().map((virtualRow) => {
                const site = sortedSites[virtualRow.index]
                return (
                  <ItemWrapper
                    $virtualRow={virtualRow}
                    $gap={gap}
                    key={virtualRow.key}
                  >
                    <LocationCard
                      key={site.id}
                      data-testid={`site-${site.id}`}
                      isSelected={site.id === selectedSite?.id}
                      scores={scoresByLocation[site.id]}
                      site={site}
                      data-index={virtualRow.index}
                      ref={(ref) => {
                        virtualizer.measureElement(ref)
                        locationCardRefs.current[site.id] = ref
                      }}
                    />
                  </ItemWrapper>
                )
              })}
            </ItemsContainer>

            {baseMapSites.length === 0 ? (
              <div css={{ height: '100%' }}>
                <NotFound>{t('plainText.noSitesFound')}</NotFound>
              </div>
            ) : (
              filteredSites.length !== baseMapSites.length && (
                <ResetButtonContainer>
                  <Button
                    background="transparent"
                    kind="secondary"
                    onClick={handleResetMapClick}
                  >
                    {upperCase(t('labels.resetMap'))}
                  </Button>
                </ResetButtonContainer>
              )
            )}
          </VirtualScrollableContainer>
        </ContainmentWrapper>
      )}
    </>
  )
}

const VirtualScrollableContainer = styled.div`
  height: 100%;
  overflow-y: auto;
  contain: strict;
`

const ItemsContainer = styled.div<{
  $virtualizer: Virtualizer<HTMLDivElement, Element>
}>(
  ({ theme, $virtualizer }) => css`
    height: ${$virtualizer.getTotalSize()}px;
    position: relative;
    margin: ${theme.spacing.s12};
  `
)

const ItemWrapper = styled.div<{
  $virtualRow: VirtualItem
  $gap: number
}>(
  ({ $virtualRow, $gap }) => css`
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    transform: translateY(${$virtualRow.start ?? 0}px);
    /* gap param in useVirtualizer will change the totalSize but doesn't
       apply the gap to layout, so need this manual process to recalculate
       the height to allow gap between LocationCards */
    height: ${$virtualRow.size + $gap}px;
  `
)

const SitesContainer = styled.div(
  ({ theme }) => css`
    margin: ${theme.spacing.s12};
    display: flex;
    flex-direction: column;
    gap: ${theme.spacing.s12};
  `
)

const ResetButtonContainer = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
`

const FetchNextPage = ({ onView, dependencies }) => {
  const endOfSiteRef = useIntersectionObserverRef(
    {
      onView,
    },
    dependencies
  )

  return (
    <div ref={endOfSiteRef}>
      {/* to be detected inside an inner div, it needs to have content or content
      after so put a non-breaking space in here */}
      {'\u00a0'}
    </div>
  )
}
