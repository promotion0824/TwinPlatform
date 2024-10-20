import React, { useMemo } from 'react'
import { useParams, useHistory } from 'react-router'
import cx from 'classnames'
import {
  AssetsList,
  Icon,
  Pill,
  Spacing,
  Table,
  Head,
  Body,
  Row,
  Cell,
  Text,
} from '@willow/mobile-ui'
import { useLayout, useFloor } from 'providers'
import styles from './Assets.css'

const Asset = React.forwardRef(
  ({ asset, floor, categories, selectedCategoryId }, ref) => {
    const history = useHistory()
    const { site } = useLayout()
    const floorContext = useFloor()

    const equipmentExistsInLayer = floor.layerGroup?.equipments.some(
      (equipment) => equipment.id === asset.equipmentId
    )

    const isDraggable =
      floor.mode === 'create' &&
      floor.layerGroup?.id !== 'floor_layer' &&
      asset.equipmentId != null &&
      !equipmentExistsInLayer

    const cxRowClassName = cx(styles.row, {
      [styles.draggable]: isDraggable,
    })

    return (
      <Row
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
          history.push(`/sites/${site.id}/floors/asset/${asset.id}`)
        }}
        data-segment="Asset Clicked"
        data-segment-props={JSON.stringify({
          category: categories.find((x) => x.id === selectedCategoryId)?.name,
          name: asset.name,
          page: 'Mobile Assets',
        })}
      >
        {!floorContext.isReadOnly && (
          <Cell type="fill">
            <Spacing align="center middle">
              <Icon icon="drag" className={styles.drag} />
            </Spacing>
          </Cell>
        )}
        <Cell>
          <Spacing>
            <Text>{asset.name}</Text>
            {!asset.isEquipmentOnly && asset.equipmentName != null && (
              <Text type="label">{asset.equipmentName}</Text>
            )}
          </Spacing>
        </Cell>
        {asset.isEquipmentOnly && (
          <Cell type="fill">
            <Spacing horizontal size="small">
              {asset.tags.map((tag) => (
                <Pill key={tag} className={styles.pill}>
                  {tag}
                </Pill>
              ))}
            </Spacing>
          </Cell>
        )}
        {!asset.isEquipmentOnly && <Cell>{asset.identifier}</Cell>}
      </Row>
    )
  }
)

export default function Assets({ categories, selectedCategoryId, search }) {
  const { site } = useLayout()
  const floorContext = useFloor()
  const { floor } = floorContext

  const AssetComponent = useMemo(() => {
    return React.forwardRef(({ asset }, ref) => {
      return (
        <Asset
          ref={ref}
          asset={asset}
          floor={floor}
          categories={categories}
          selectedCategoryId={selectedCategoryId}
        />
      )
    })
  }, [floor, categories, selectedCategoryId])

  if (selectedCategoryId == null && search === '') {
    return null
  }

  return (
    <Table>
      <Head>
        <Row>
          {!floorContext.isReadOnly && <Cell width={32} />}
          <Cell sort="name">Name</Cell>
          <Cell>Id / Tags</Cell>
        </Row>
      </Head>
      <Body>
        <AssetsList
          params={{
            siteId: site.id,
            categoryId: selectedCategoryId,
            floorId:
              floor.name !== 'BLDG' && floor.name !== 'SOFI CAMPUS OVERALL'
                ? floor.id
                : undefined,
            liveDataAssetsOnly: !floorContext.isReadOnly,
            searchKeyword: search !== '' ? search : undefined,
          }}
          AssetComponent={AssetComponent}
        />
      </Body>
    </Table>
  )
}
