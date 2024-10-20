import { useState } from 'react'
import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useApi } from '@willow/ui'
import Table, { Body, Cell, Head, Row } from '../Table/Table'
import InspectionsRow from './InspectionsRow'

export default function InspectionsTable({ inspections, onSelect, onArchive }) {
  const api = useApi()
  const params = useParams()
  const { t } = useTranslation()

  const [items, setItems] = useState(inspections)
  const [changeOrderCallIsActive, setChangeOrderCallIsActive] = useState(false)

  const changeInspectionSortOrder = async (inspectionId, direction) => {
    setChangeOrderCallIsActive(true)
    const inspectionsBySortOrder =
      inspections.sort((a, b) => {
        if (a.sortOrder < b.sortOrder) {
          return -1
        }
        if (a.sortOrder > b.sortOrder) {
          return 1
        }
        return 0
      }) || []
    const index = inspectionsBySortOrder.findIndex((x) => x.id === inspectionId)
    const directionOperator = direction === 'up' ? -1 : 1
    const indexToSwap = index + directionOperator

    // Change sortOrder for front-end refresh
    inspectionsBySortOrder[index].sortOrder += directionOperator
    inspectionsBySortOrder[indexToSwap].sortOrder -= directionOperator

    // Change sort of items so we can recreate list of ids for back-end update
    const temp = inspectionsBySortOrder[index]
    inspectionsBySortOrder[index] = inspectionsBySortOrder[indexToSwap]
    inspectionsBySortOrder[indexToSwap] = temp

    await api.put(
      // once scope selector feature is turned on, we no long have siteId in params
      // so we grab it from the inspection object where it's always available
      `/api/sites/${params.siteId || temp.siteId}/zones/${
        params.zoneId
      }/inspections/sortOrder`,
      {
        inspectionIds: inspectionsBySortOrder.map((x) => x.id),
      }
    )

    setItems([...inspectionsBySortOrder])
    setChangeOrderCallIsActive(false)
  }

  return (
    <Table
      items={items}
      notFound={t('plainText.noInspectionsFound')}
      sort="sortOrder"
      style={{
        // Override display: flex on Table component, so that table row height
        // doesn't expand to take full screen height. The table header row is sticky.
        display: 'initial',
      }}
    >
      {(tableItems) => (
        <>
          <Head>
            <Row>
              <Cell width={30} />
              <Cell sort="floorCode" width={70}>
                {t('labels.floor')}
              </Cell>
              <Cell sort="name" width={200}>
                {t('plainText.inspection')}
              </Cell>
              <Cell width={90}>{t('plainText.checks')}</Cell>
              <Cell sort="assetName">{t('plainText.asset')}</Cell>
              <Cell width={100} />
              <Cell width={50} />
            </Row>
          </Head>
          <Body>
            {tableItems.map((inspection, index) => (
              <InspectionsRow
                key={inspection.id}
                inspection={inspection}
                index={index}
                totalItems={tableItems.length}
                onSelect={() => onSelect(inspection)}
                onArchive={() => onArchive(inspection)}
                changeInspectionSortOrder={changeInspectionSortOrder}
                changeOrderCallIsActive={changeOrderCallIsActive}
              />
            ))}
          </Body>
        </>
      )}
    </Table>
  )
}
