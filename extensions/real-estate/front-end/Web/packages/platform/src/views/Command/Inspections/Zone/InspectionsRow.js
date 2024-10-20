import cx from 'classnames'
import {
  AssetPill,
  Button,
  Flex,
  FloorPill,
  Icon,
  MoreButton,
  MoreDropdownButton,
  Pill,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useTable, Row, Cell } from '../Table/Table'
import CheckRow from './CheckRow'
import styles from './InspectionsRow.css'

export default function InspectionsRow({
  inspection,
  onSelect,
  onArchive,
  changeInspectionSortOrder,
  changeOrderCallIsActive,
  index,
  totalItems,
}) {
  const table = useTable()
  const { t } = useTranslation()

  const isOpen = table.isOpen(inspection.id)
  const isSortedBySortOrderColumn = table.sortState.sort === 'sortOrder'
  const isSortedAscending = table.sortState.order === 'asc'

  const cxClassName = cx({
    [styles.isOpen]: isOpen,
  })

  const handleUpSortOrderClick = (event, id) => {
    event.stopPropagation()
    changeInspectionSortOrder(id, isSortedAscending ? 'up' : 'down')
  }

  const handleDownSortOrderClick = (event, id) => {
    event.stopPropagation()
    changeInspectionSortOrder(id, isSortedAscending ? 'down' : 'up')
  }

  return (
    <>
      <Row
        className={cxClassName}
        selected={isOpen}
        selectedType="basic"
        onClick={() => table.toggleIsOpen(inspection.id)}
      >
        <Cell type="fill">
          <Flex>
            <Icon icon="chevron" className={styles.chevron} />
          </Flex>
        </Cell>
        <Cell>
          <FloorPill>{inspection.floorCode}</FloorPill>
        </Cell>
        <Cell>
          <Pill>{inspection.name}</Pill>
        </Cell>
        <Cell>
          <Pill>
            <Flex horizontal>
              <div>
                <Icon
                  icon="checkbox"
                  size="extraTiny"
                  className={styles.numChecks}
                />
              </div>
              {inspection.checks.length}
            </Flex>
          </Pill>
        </Cell>
        <Cell>
          <AssetPill>{inspection.assetName}</AssetPill>
        </Cell>
        <Cell type="none" className={styles.sortOrderCell} width={100}>
          <div
            className={styles.disabledTooltip}
            data-tooltip={
              !isSortedBySortOrderColumn
                ? 'Sort inspections by SORT ORDER column to enable sorting'
                : ''
            }
          >
            <Button
              data-tooltip={
                isSortedAscending
                  ? 'Increase sort order'
                  : 'Decrease sort order'
              }
              icon="up"
              iconSize="small"
              onClick={(event) => handleUpSortOrderClick(event, inspection.id)}
              disabled={
                index === 0 ||
                !isSortedBySortOrderColumn ||
                changeOrderCallIsActive
              }
              className={
                !isSortedBySortOrderColumn ? styles.buttonDisabled : {}
              }
            />
          </div>
          <div
            className={styles.disabledTooltip}
            data-tooltip={
              !isSortedBySortOrderColumn
                ? 'Sort inspections by SORT ORDER column to enable sorting'
                : ''
            }
          >
            <Button
              data-tooltip={
                isSortedAscending
                  ? 'Decrease sort order'
                  : 'Increase sort order'
              }
              icon="down"
              iconSize="small"
              onClick={(event) =>
                handleDownSortOrderClick(event, inspection.id)
              }
              disabled={
                index === totalItems - 1 ||
                !isSortedBySortOrderColumn ||
                changeOrderCallIsActive
              }
              className={
                !isSortedBySortOrderColumn ? styles.buttonDisabled : {}
              }
            />
          </div>
        </Cell>
        <Cell type="none" width={50}>
          <MoreButton>
            <MoreDropdownButton
              icon="right"
              onClick={onSelect}
              data-segment="Inspection Settings"
              data-segment-props={JSON.stringify({
                level: inspection.floorCode,
                asset: inspection.assetName,
                name: inspection.name,
                zone: inspection.zoneName,
                page: 'Zone',
              })}
            >
              {t('plainText.inspectionSettings')}
            </MoreDropdownButton>
            <MoreDropdownButton
              icon="trash"
              onClick={onArchive}
              data-segment="Archive Inspection"
              data-segment-props={JSON.stringify({
                level: inspection.floorCode,
                asset: inspection.assetName,
                name: inspection.name,
                zone: inspection.zoneName,
                page: 'Zone',
              })}
            >
              {t('headers.archiveInspection')}
            </MoreDropdownButton>
          </MoreButton>
        </Cell>
      </Row>
      {isOpen && <CheckRow inspection={inspection} />}
    </>
  )
}
