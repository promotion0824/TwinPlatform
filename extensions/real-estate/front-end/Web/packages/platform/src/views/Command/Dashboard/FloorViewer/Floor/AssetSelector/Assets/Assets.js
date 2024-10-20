import React, { useMemo } from 'react'
import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import cx from 'classnames'
import {
  AssetsList,
  Flex,
  Icon,
  Message,
  NotFound,
  Progress,
  Text,
} from '@willow/ui'
import { styled } from 'twin.macro'
import Table, { Body, Row, Cell } from './Table/Table'
import { useAssetSelector } from '../AssetSelectorContext'
import { useFloor } from '../../FloorContext'
import styles from './Assets.css'

const Asset = React.forwardRef(
  ({ floor, asset, selectedCategory, assetSelector, onAssetIdChange }, ref) => {
    const equipmentExistsOnFloor = floor.layerGroups
      .flatMap((layerGroup) => layerGroup.equipments)
      .some(
        (equipment) =>
          equipment.id === asset.equipmentId || equipment.id === asset.id
      )

    const isDraggable =
      !floor.isReadOnly &&
      floor.layerGroup?.id !== 'floor_layer' &&
      !equipmentExistsOnFloor
    const cxRowClassName = cx(styles.row, {
      [styles.draggable]: isDraggable,
    })

    return (
      <StyledRow
        ref={ref}
        key={asset.id}
        draggable={isDraggable}
        selected={floor.selectedAsset?.id === asset.id}
        className={cxRowClassName}
        onDragStart={(e) => {
          if (!isDraggable) {
            e.preventDefault()
            return
          }

          e.dataTransfer.setData(
            'text/plain',
            JSON.stringify({
              ...asset,
              id: asset.equipmentId ?? asset.id,
              name: asset.equipmentName ?? asset.name,
            })
          )
        }}
        onClick={() => {
          if (floor.isReadOnly) {
            floor.selectAsset({
              ...asset,
              moduleTypeNamePath: floor.formatModuleTypeNamePath(
                asset.moduleTypeNamePath
              ),
            })
            floor.showSelectedAsset()

            const layerId = floor.getMain3dLayerForModuleTypeName(
              asset?.moduleTypeNamePath
            )?.id

            const levelImageId = floor.getMain2dImageForModuleTypeName(
              asset?.moduleTypeNamePath
            )?.id

            if (floor.selectedAsset?.id !== asset?.id) {
              if (
                layerId != null &&
                !floor.selectedLayerIds.includes(layerId)
              ) {
                floor.setSelectedLayerIds((prevSelectedLayerIds) => [
                  ...prevSelectedLayerIds,
                  layerId,
                ])
              }

              if (levelImageId != null) {
                floor.selectImage(levelImageId)
              }
            }

            const nextSelectedAsset =
              floor.selectedAsset?.id !== asset?.id ? asset : undefined
            const nextAsset =
              nextSelectedAsset?.id != null &&
              nextSelectedAsset?.forgeViewerModelId != null
                ? {
                    assetId: nextSelectedAsset?.id,
                    forgeViewerAssetId:
                      nextSelectedAsset?.forgeViewerModelId?.toLowerCase?.(),
                  }
                : undefined

            floor.iframeRef.current?.contentWindow?.selectAsset?.(nextAsset)
            onAssetIdChange(asset.id)
          }
        }}
        data-segment="Asset Clicked"
        data-segment-props={JSON.stringify({
          category: selectedCategory?.name,
          name: asset.equipmentName ?? asset.name,
          search_term: assetSelector.search,
          page: 'Floor Dashboard',
        })}
      >
        {!floor.isReadOnly && (
          <Cell type="fill">
            <Flex align="center middle">
              <Icon icon="drag" className={styles.drag} />
            </Flex>
          </Cell>
        )}
        <Cell
          css={`
            &&& {
              padding: 10px 16px;
            }
          `}
        >
          <Flex>
            {asset.identifier != null && (
              <Text type="message" size="tiny" color="grey">
                {asset.identifier}
              </Text>
            )}
            <div>{asset.equipmentName ?? asset.name}</div>
          </Flex>
        </Cell>
      </StyledRow>
    )
  }
)

export default function Assets({
  assetId,
  selectedAssetQuery,
  onSelectedAssetIdChange,
}) {
  const assetSelector = useAssetSelector()
  const floor = useFloor()
  const params = useParams()
  const { t } = useTranslation()

  const selectedCategory = assetSelector.categories.slice(-1)[0]

  const AssetComponent = useMemo(() => {
    return React.forwardRef(({ asset }, ref) => {
      return (
        <Asset
          ref={ref}
          asset={asset}
          floor={floor}
          selectedCategory={selectedCategory}
          assetSelector={assetSelector}
          assetId={assetId}
          onAssetIdChange={onSelectedAssetIdChange}
        />
      )
    })
  }, [floor])

  if (
    (selectedCategory == null || selectedCategory.assetCount === 0) &&
    assetSelector.search === '' &&
    assetId == null
  ) {
    return null
  }

  return (
    <div className={styles.assets}>
      <Table>
        <Body>
          {/**
           * If assetId is present and search asset name
           * is empty then return only specific asset details
           * else return the entire asset list based on searched asset name from the API
           */}
          {assetId &&
          selectedAssetQuery &&
          assetSelector.search === '' &&
          (selectedCategory == null || selectedCategory.assetCount === 0) ? (
            // Showing Asset not found message when user changes the assetId which is not valid
            selectedAssetQuery.status === 'error' ? (
              <Message icon="error">
                {selectedAssetQuery.error?.response?.status === 400
                  ? t('plainText.assetNotFound')
                  : t('plainText.errorOccurred')}
              </Message>
            ) : selectedAssetQuery.status === 'loading' ||
              selectedAssetQuery.status === 'idle' ? (
              <Progress />
            ) : selectedAssetQuery.status === 'success' &&
              selectedAssetQuery.data == null ? (
              <NotFound>{t('plainText.pageNotFound')}</NotFound>
            ) : (
              <Asset
                asset={selectedAssetQuery.data}
                floor={floor}
                selectedCategory={selectedCategory}
                assetSelector={assetSelector}
                assetId={assetId}
                onAssetIdChange={onSelectedAssetIdChange}
              />
            )
          ) : (
            <AssetsList
              params={{
                siteId: params.siteId,
                floorId: floor?.isSiteWide ? undefined : params.floorId,
                categoryId: selectedCategory?.id,
                searchKeyword: assetSelector.search || undefined,
              }}
              AssetComponent={AssetComponent}
              // Because `AssetComponent`'s top-level component is a `Row`, which
              // has `display: contents`.
              getObservableElements={(node) => node.children}
            />
          )}
        </Body>
      </Table>
    </div>
  )
}

const StyledRow = styled(Row)(({ theme }) => ({
  '&&& > td': {
    '&:hover': {
      backgroundColor: theme.color.neutral.bg.panel.hovered,
    },
  },
}))
