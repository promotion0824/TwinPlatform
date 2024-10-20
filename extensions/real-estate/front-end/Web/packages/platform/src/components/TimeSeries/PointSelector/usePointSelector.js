/* eslint-disable complexity */
import { debounce } from 'lodash'
import { Fragment, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'

import {
  Flex,
  Icon as LegacyIcon,
  NotFound,
  useAnalytics,
  useFeatureFlag,
} from '@willow/ui'
import { Button, Icon, Modal, SearchInput } from '@willowinc/ui'

import { useSites } from '../../../providers/index'
import routes from '../../../routes'
import { useTimeSeries } from '../TimeSeriesContext'
import Asset from './Asset'
import AssetModal from './AssetModal/AssetModal'

export default function usePointSelector({
  modalTab,
  modalAssetId,
  insightId,
  onModalTabChange,
  onAssetChange,
  onInsightIdChange,
  setIsAssetSelectorModalOpen,
  selectedTicketId,
  onSelectedTicketIdChange,
  insightTab,
  onInsightTabChange,
  openSearchModal,
}) {
  const sites = useSites()
  const analytics = useAnalytics()
  const featureFlag = useFeatureFlag()
  const timeSeries = useTimeSeries()
  const { t } = useTranslation()
  const [sitesCount, setSitesCount] = useState(0)
  const [search, setSearch] = useState('')
  const [searchInput, setSearchInput] = useState('')

  const [isRemoveAssets, setIsRemoveAssets] = useState(false)
  const filteredAssets = timeSeries.assets
    .map((asset) => ({
      ...asset,
      data:
        asset.data != null
          ? {
              ...asset.data,
              points: asset.data.points.filter((point) => {
                const isSelected = timeSeries.points.some(
                  (prevPoint) => prevPoint.sitePointId === point.sitePointId
                )

                return (
                  isSelected ||
                  point.externalPointId
                    .toLowerCase()
                    .includes(search.toLowerCase()) ||
                  point.name.toLowerCase().includes(search.toLowerCase())
                )
              }),
            }
          : undefined,
    }))
    .filter((asset) => asset.data == null || asset.data?.points.length > 0)

  useEffect(() => {
    const sitesIds = new Set(
      filteredAssets.map((filteredAsset) => filteredAsset.siteId)
    )
    setSitesCount(Array.from(sitesIds).length)
  }, [filteredAssets])

  useEffect(() => {
    analytics.track('Time Series Current View Search Filter', { search })
  }, [search])

  function handleHideAll() {
    timeSeries.points.forEach((point) => {
      timeSeries.toggleSitePointId(point.sitePointId)
    })
    analytics.track('Time Series Current View Hide All')
  }

  function handleRemoveAll() {
    setIsRemoveAssets(true)
    analytics.track('Time Series Item Remove All')
  }

  function handleRemoveSubmit() {
    setIsRemoveAssets(false)
    timeSeries.reset()
  }

  function handleShowAll() {
    analytics.track('Time Series Current View Show All')
    const activePoints = timeSeries.points.map((point) => point.sitePointId)
    filteredAssets.forEach((asset) => {
      asset.data.points.forEach((point) => {
        if (!activePoints.includes(`${asset.siteId}_${point.entityId}`)) {
          timeSeries.toggleSitePointId(`${asset.siteId}_${point.entityId}`)
        }
      })
    })
  }

  function onClose() {
    setIsRemoveAssets(false)

    if (!timeSeries.assets.length) {
      // This timeout is used to allow for a smoother transition when opening the modal
      setTimeout(
        featureFlag.hasFeatureToggle('timeSeriesSearchModal')
          ? openSearchModal()
          : setIsAssetSelectorModalOpen(true),
        3000
      )
    }
  }

  /**
   * cycle thru filteredAssets list so that
   * - handler will target first assetId when current assetId is last
   * in the list and user click on "next" button
   * - handler will target last assetId when current assetId is first
   * in list and user click on "prev" button
   * - target next or previous assetId when current assetId is neither
   * first nor last and user click on "next" or "prev" button respectively
   * */
  const handleAssetModalChange = ({ asset, modalTab, direction = 'next' }) => {
    const lastIndex = filteredAssets.length - 1
    const assetIndex = filteredAssets
      .map((asset) => asset.assetId)
      .indexOf(asset.assetId)
    let nextIndex

    if (direction === 'next') {
      nextIndex = assetIndex === lastIndex ? 0 : assetIndex + 1
    } else {
      nextIndex = assetIndex === 0 ? lastIndex : assetIndex - 1
    }

    onAssetChange({
      modalAssetId: filteredAssets[nextIndex]?.assetId,
      modalTab,
    })
  }

  const activeAsset = filteredAssets.find(
    (asset) => asset?.assetId === modalAssetId
  )
  const activeAssetSite = sites?.find((s) => s.id === activeAsset?.siteId)
  const assetModalHeader =
    activeAsset?.data?.name && activeAssetSite?.name
      ? `${activeAsset.data.name} - ${activeAssetSite.name}`
      : `${activeAsset?.data?.name ?? ''}${activeAssetSite?.name ?? ''}`

  const debounceSearch = debounce((input) => {
    setSearch(input)
  }, 300 /* same as previous default */)

  const handleSearchInputChange = (e) => {
    const { value } = e.target
    setSearchInput(value)
    debounceSearch(value)
  }

  return [
    <Container>
      <SearchContainer>
        <ControlContainer>
          <SearchInput
            placeholder={t('plainText.filterDataPoints')}
            value={searchInput}
            onChange={handleSearchInputChange}
            css={{ width: '100%' }}
          />
          {timeSeries.assets.length > 0 && !search && (
            <Button
              prefix={<Icon icon="visibility_off" />}
              kind="secondary"
              onClick={handleHideAll}
              disabled={timeSeries.points.length === 0}
            >
              {t('plainText.hideAll')}
            </Button>
          )}

          {search && (
            <Button
              prefix={<Icon icon="visibility" />}
              kind="secondary"
              onClick={handleShowAll}
              disabled={filteredAssets.length === 0}
            >
              {t('plainText.selectAll')}
            </Button>
          )}
        </ControlContainer>

        {timeSeries.assets.length > 0 && (
          <span
            css={css(({ theme }) => ({
              ...theme.font.body.sm.regular,
              color: theme.color.neutral.fg.subtle,
            }))}
          >
            {t('interpolation.showingDataPointsFromTwins', {
              numOfDataPoints: timeSeries?.points?.length,
              numOfTwins: timeSeries?.assets?.length,
            })}
          </span>
        )}
      </SearchContainer>

      <AssetContainer>
        <Flex>
          {filteredAssets.map((asset, i) => (
            <Fragment key={asset.assetId}>
              {asset.data != null ? (
                <Asset
                  asset={asset}
                  isSearching={search.length > 0}
                  search={search}
                  sitesCount={sitesCount}
                  onAssetChange={onAssetChange}
                />
              ) : (
                <Flex align="center" padding="extraLarge">
                  <LegacyIcon icon="progress" />
                </Flex>
              )}
            </Fragment>
          ))}
        </Flex>
        {timeSeries.assets.length > 0 && filteredAssets.length === 0 && (
          <Flex>
            <NotFound>{t('plainText.noAssetsFound')}</NotFound>
          </Flex>
        )}
      </AssetContainer>

      {activeAsset && modalAssetId && (
        <AssetModal
          t={t}
          selectedTicketId={selectedTicketId}
          onSelectedTicketIdChange={onSelectedTicketIdChange}
          onClose={() =>
            onAssetChange({
              modalAssetId: undefined,
              modalTab: undefined,
            })
          }
          assetId={activeAsset?.assetId}
          siteId={activeAsset?.siteId}
          insightId={insightId}
          onInsightIdChange={onInsightIdChange}
          onPreviousItem={() =>
            handleAssetModalChange({
              asset: activeAsset,
              modalTab,
              direction: 'prev',
            })
          }
          onNextItem={() =>
            handleAssetModalChange({
              asset: activeAsset,
              modalTab,
            })
          }
          onModalTabChange={onModalTabChange}
          selectedModalTab={modalTab}
          insightTab={insightTab}
          onInsightTabChange={onInsightTabChange}
          // display header
          // - in form of "Asset Name" - "Site Name" when both are defined
          // - in form of just "Asset Name" or  just "Site Name" when assetName or siteName is defined
          // - in form of empty string when both aren't defined
          header={assetModalHeader}
        />
      )}
    </Container>,
    <FooterContainer>
      <Button
        prefix={<Icon icon="delete" />}
        kind="secondary"
        onClick={handleRemoveAll}
        disabled={timeSeries.assets.length === 0 || search}
        data-testid="remove-all-button"
      >
        {t('plainText.removeAll')}
      </Button>
      <Modal
        header={t('headers.warning')}
        onClose={onClose}
        opened={isRemoveAssets}
        size="sm"
        centered
      >
        <div
          css={css(({ theme }) => ({
            padding: theme.spacing.s16,
            gap: theme.spacing.s8,
            display: 'flex',
            flexDirection: 'column',
          }))}
        >
          {t('questions.sureToRemoveAssets')}
          <div
            css={css(({ theme }) => ({
              gap: theme.spacing.s8,
              display: 'flex',
              flexDirection: 'row',
              alignSelf: 'flex-end',
            }))}
          >
            <Button kind="secondary" onClick={() => setIsRemoveAssets(false)}>
              Cancel
            </Button>
            <Button kind="negative" onClick={handleRemoveSubmit}>
              {t('plainText.removeAll')}
            </Button>
          </div>
        </div>
      </Modal>

      <Button
        prefix={<Icon icon="add" />}
        data-testid="add-twin-button"
        {...(featureFlag.hasFeatureToggle('timeSeriesSearchModal')
          ? {
              onClick: openSearchModal,
            }
          : {
              onClick: (e) => {
                setIsAssetSelectorModalOpen(true)
                e.preventDefault() // so click it won't reload the page
              },
              href: routes.timeSeries_addTwin,
            })}
      >
        {t('plainText.addTwin')}
      </Button>
    </FooterContainer>,
  ]
}

const Container = styled.div`
  height: 100%;
  display: flex;
  flex-direction: column;
`
const SearchContainer = styled.div(
  ({ theme }) => css`
    padding: ${theme.spacing.s12} ${theme.spacing.s16} ${theme.spacing.s8};

    display: flex;
    justify-content: center;
    flex-direction: column;
    gap: ${theme.spacing.s8};
  `
)

const ControlContainer = styled.div(
  ({ theme }) => css`
    width: 100%;

    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: ${theme.spacing.s8};
  `
)
const AssetContainer = styled.div`
  flex-grow: 1;
  overflow-y: auto;
`

const FooterContainer = styled.div(
  ({ theme }) => css`
    width: 100%;
    display: flex;
    gap: ${theme.spacing.s8};
    justify-content: end;
    align-items: center;
    color: ${theme.color.neutral.fg.default};
  `
)
