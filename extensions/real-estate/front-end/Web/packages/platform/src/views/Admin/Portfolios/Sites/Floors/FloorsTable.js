import { titleCase, useEffectOnceMounted } from '@willow/common'
import {
  Blocker,
  Body,
  Button,
  Cell,
  Head,
  Row,
  Table,
  useApi,
  useSnackbar,
} from '@willow/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'

export default function FloorsTable({
  floors,
  selectedFloor,
  setSelectedFloor,
}) {
  const api = useApi()
  const params = useParams()
  const snackbar = useSnackbar()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [sortedFloorIds, setSortedFloorIds] = useState(() =>
    floors.map((floor) => floor.id)
  )
  const [isProcessing, setIsProcessing] = useState(false)

  useEffectOnceMounted(() => {
    async function save() {
      try {
        setIsProcessing(true)
        await api.put(
          `/api/sites/${params.siteId}/floors/sortorder`,
          [...sortedFloorIds].reverse()
        )
        setIsProcessing(false)
      } catch (err) {
        setIsProcessing(false)

        snackbar.show(t('plainText.errorOccurred'))
      }
    }

    save()
  }, [sortedFloorIds])

  const sortedFloors = sortedFloorIds.map((floorId) =>
    floors.find((floor) => floor.id === floorId)
  )

  function handleUpSortOrderClick(e, floorId) {
    e.stopPropagation()

    setSortedFloorIds((prevSortedFloorIds) => {
      const floorIdIndex = prevSortedFloorIds.indexOf(floorId)
      const prevFloorId = prevSortedFloorIds[floorIdIndex - 1]

      return [
        ...prevSortedFloorIds.slice(0, Math.max(floorIdIndex - 1, 0)),
        ...[floorId, prevFloorId].filter((id) => id != null),
        ...prevSortedFloorIds.slice(floorIdIndex + 1),
      ]
    })
  }

  function handleDownSortOrderClick(e, floorId) {
    e.stopPropagation()

    setSortedFloorIds((prevSortedFloorIds) => {
      const floorIdIndex = prevSortedFloorIds.indexOf(floorId)
      const nextFloorId = prevSortedFloorIds[floorIdIndex + 1]

      return [
        ...prevSortedFloorIds.slice(0, floorIdIndex),
        ...[nextFloorId, floorId].filter((id) => id != null),
        ...prevSortedFloorIds.slice(floorIdIndex + 2),
      ]
    })
  }

  return (
    <>
      <Table items={sortedFloors} notFound={t('plainText.noFloorsFound')}>
        <Head>
          <Row>
            <Cell width="150px">{t('labels.code')}</Cell>
            <Cell width="150px">{t('labels.floor')}</Cell>
            <Cell width="150px">
              {titleCase({ language, text: t('labels.sitewide') })}
            </Cell>
            <Cell width="1fr">{t('plainText.modelRef')}</Cell>
            <Cell>{t('plainText.sortOrder')}</Cell>
          </Row>
        </Head>
        <Body>
          {sortedFloors.map((floor, i) => (
            <Row
              key={floor.id}
              selected={selectedFloor === floor}
              onClick={() => setSelectedFloor(floor)}
            >
              <Cell>{floor.code}</Cell>
              <Cell>{floor.name}</Cell>
              <Cell>{floor?.isSiteWide ? 'Yes' : ''}</Cell>
              <Cell>{floor?.modelReference}</Cell>
              <Cell type="fill">
                <Button
                  icon="up"
                  iconSize="small"
                  disabled={i === 0}
                  readOnly={i === 0}
                  data-tooltip={i !== 0 ? 'Increase sort order' : undefined}
                  data-tooltip-position="bottom"
                  onClick={(e) => handleUpSortOrderClick(e, floor.id)}
                />
                <Button
                  icon="down"
                  iconSize="small"
                  disabled={i === sortedFloors.length - 1}
                  readOnly={i === sortedFloors.length - 1}
                  data-tooltip={
                    i !== sortedFloors.length - 1
                      ? 'Decrease sort order'
                      : undefined
                  }
                  data-tooltip-position="bottom"
                  onClick={(e) => handleDownSortOrderClick(e, floor.id)}
                />
              </Cell>
            </Row>
          ))}
        </Body>
      </Table>
      {isProcessing && <Blocker />}
    </>
  )
}
