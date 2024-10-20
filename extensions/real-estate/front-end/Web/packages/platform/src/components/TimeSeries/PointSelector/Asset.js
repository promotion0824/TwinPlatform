import _ from 'lodash'
import { useSites } from 'providers'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'

import {
  MoreButton,
  MoreDropdownButton,
  Text,
  TooltipWhenTruncated,
  useAnalytics,
  useSnackbar,
  VanillaButton,
} from '@willow/ui'
import { Icon } from '@willowinc/ui'

import { useTimeSeries } from '../TimeSeriesContext'
import { AssetModalTab } from './AssetModal/AssetModal'
import Dot from './Dot'
import Point from './Point'

export default function Asset({
  asset,
  isSearching,
  sitesCount,
  search,
  onAssetChange,
}) {
  const analytics = useAnalytics()
  const sites = useSites()
  const snackbar = useSnackbar()
  const timeSeries = useTimeSeries()
  const { t } = useTranslation()
  const isOpen = isSearching ? true : timeSeries.activeAssetId === asset.assetId
  const filterSelectedPoints = timeSeries.points.filter(
    (pointAsset) => pointAsset.assetId === asset.assetId
  )
  asset.data.points.filter((searchPoint) =>
    searchPoint.name.includes(search.toLowerCase())
  )
  const site = sites.find((layoutSite) => layoutSite.id === asset.data.siteId)
  const totalOpenPoints = filterSelectedPoints.length

  const selectedPointIds = timeSeries.points.map((point) => point.pointId)

  // We may be given multiple points with the same id, which causes issues.
  // Remove duplicates.
  const uniquePoints = _.uniqBy(asset.data.points, (p) => p.id).sort((a, b) =>
    a.name.toLowerCase() < b.name.toLowerCase() ? -1 : 1
  )

  const unSelectedPoints = uniquePoints.filter(
    (point) => !selectedPointIds.includes(point.entityId)
  )

  const selectedPoints = uniquePoints.filter((point) =>
    selectedPointIds.includes(point.entityId)
  )

  function handleAssetCollapseClick() {
    timeSeries.openClickedAsset(asset.assetId)
    if (isOpen) {
      analytics.track('Time Series Item Collapsed', {
        item_name: asset?.data?.name,
      })
      return
    }
    analytics.track('Time Series Item Expanded', {
      item_name: asset?.data?.name,
    })
  }

  function handleMoreButtonClick({ modalAssetId, modalTab }) {
    onAssetChange({ modalAssetId, modalTab })
    analytics.track('Time Series Item Go To', {
      Site: site,
      item_name: asset.data.name,
      option: modalTab,
    })
  }

  function handleRemoveAsset() {
    timeSeries.addOrRemoveAsset(asset.siteAssetId)
    timeSeries.removeSitePoints(
      selectedPoints.map((point) => point.sitePointId)
    )
    snackbar.show(
      () => <Text size="tiny">{t('plainText.assetRemoved')}</Text>,
      {
        icon: '',
      }
    )
  }

  return (
    <AssetContainer>
      <ToggleAssetButton
        type="button"
        onClick={handleAssetCollapseClick}
        $isOpen={isOpen}
      >
        <AssetSummaryContainer>
          <Icon icon={isOpen ? 'arrow_drop_down' : 'arrow_right'} />
          <TextContainer $isOpen={isOpen}>
            {sitesCount > 1 && (
              <TooltipWhenTruncated label={site?.name}>
                <span
                  css={css(({ theme }) => ({
                    ...theme.font.body.xs.regular,
                    textTransform: 'uppercase',
                  }))}
                >
                  {site?.name}
                </span>
              </TooltipWhenTruncated>
            )}
            <TooltipWhenTruncated label={asset.data.name}>
              <span
                css={css(({ theme }) => ({
                  ...theme.font.body.md.semibold,
                }))}
              >
                {asset.data.name}
              </span>
            </TooltipWhenTruncated>
          </TextContainer>
          <DotsContainer>
            {filterSelectedPoints.slice(0, 4).map((selectedPoint) => (
              <Dot
                key={selectedPoint.sitePointId}
                color={selectedPoint.color}
              />
            ))}
            {filterSelectedPoints.length > 4 && (
              <span
                css={css(({ theme }) => ({
                  ...theme.font.body.md.regular,
                  color: theme.color.neutral.fg.muted,
                }))}
              >
                {' '}
                + {totalOpenPoints - 4}
              </span>
            )}
          </DotsContainer>
        </AssetSummaryContainer>

        <MoreButton
          data-testid={`more-button-${asset?.assetId ?? ''}`}
          css={{
            width: 20,
            height: 20,
            visibility: 'hidden',
            marginLeft: 'auto',
            '&, svg': { transition: 'none' },
          }}
        >
          {Object.entries(AssetModalTab).map(([key, tab]) => (
            <MoreDropdownButton
              key={key}
              icon="open"
              onClick={() =>
                handleMoreButtonClick({
                  modalAssetId: asset.assetId,
                  modalTab: tab,
                })
              }
              data-testid={`more-button-${tab}`}
            >
              <Text>{t(`plainText.goToAsset${_.capitalize(tab)}`)}</Text>
            </MoreDropdownButton>
          ))}
          <MoreDropdownButton
            icon="trash"
            onClick={() => handleRemoveAsset()}
            data-testid="more-button-remove"
          >
            <Text>{t('plainText.removeFromView')}</Text>
          </MoreDropdownButton>
        </MoreButton>
      </ToggleAssetButton>

      {isSearching && (
        <>
          {uniquePoints.map((point) => (
            <Point key={point.id} point={point} asset={asset} />
          ))}
        </>
      )}
      {isOpen && !isSearching && (
        <>
          {selectedPoints.map((point) => (
            <Point key={point.id} point={point} asset={asset} />
          ))}
          {unSelectedPoints.map((point) => (
            <Point key={point.id} point={point} asset={asset} />
          ))}
        </>
      )}
    </AssetContainer>
  )
}

const AssetContainer = styled.div`
  display: flex;
  flex-direction: column;
`

const ToggleAssetButton = styled(VanillaButton)(
  ({ theme, $isOpen }) => css`
    width: 100%;
    padding: ${theme.spacing.s8};
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: ${theme.spacing.s8};

    ${$isOpen &&
    // stick the toggle row when it's open
    css`
      position: sticky;
      top: 0;
      background-color: ${theme.color.neutral.bg.panel.default};
    `}

    &:hover {
      button {
        visibility: visible;
      }
    }
  `
)

const AssetSummaryContainer = styled.div`
  min-width: 0;
  display: flex;
  gap: 10px;
  align-items: center;
  justify-content: flex-start;
`

const TextContainer = styled.div(
  ({ theme, $isOpen }) => css`
    min-width: 0;
    text-align: start;
    color: ${$isOpen
      ? theme.color.neutral.fg.default
      : theme.color.neutral.fg.subtle};

    > * {
      display: block;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  `
)

const DotsContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: ${theme.spacing.s8};
    flex-shrink: 0;
  `
)
