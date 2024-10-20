import { useState } from 'react'
import { priorities } from '@willow/common'
import { Pill, Table, Head, Body, Row, Cell, Time, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styles from './HistoryTable.css'

export default function HistoryTable({ items, selectedItem, setSelectedItem }) {
  const { t } = useTranslation()
  const [hoverIndex, setHoverIndex] = useState(null)

  function getHistoryItemTitle(historyItem) {
    switch (historyItem.assetHistoryType) {
      case 'standardTicket':
      case 'scheduledTicket':
        return (
          <>
            <div className={styles.activeText}>{historyItem.issueName}</div>
            <div>
              <Text size="small">{historyItem.sequenceNumber}</Text>
            </div>
          </>
        )
      case 'inspection':
        return (
          <>
            <div className={styles.activeText}>{historyItem.assetName}</div>
            <div>
              <Text size="small">{historyItem.name}</Text>
            </div>
          </>
        )
      case 'insight':
        return (
          <>
            <div className={styles.activeText}>{historyItem.name}</div>
            <div>
              <Text size="small">{historyItem.sequenceNumber}</Text>
            </div>
          </>
        )
      default:
        return <></>
    }
  }

  function getHistoryItemClosedDate(historyItem) {
    switch (historyItem.assetHistoryType) {
      case 'standardTicket':
      case 'scheduledTicket':
        return historyItem.closedDate
      case 'inspection':
        return historyItem.nextCheckRecordDueTime
      case 'insight':
        return historyItem.updatedDate
      default:
        return historyItem.updatedDate
    }
  }

  function getHistoryItemType(historyItem, isHovered) {
    switch (historyItem.assetHistoryType) {
      case 'standardTicket':
        return isHovered
          ? t('plainText.assetHistoryStandardTicket').toUpperCase()
          : t('plainText.standard').toUpperCase()
      case 'scheduledTicket':
        return isHovered
          ? t('plainText.assetHistoryScheduledTicket').toUpperCase()
          : t('plainText.scheduled').toUpperCase()
      case 'inspection':
        return t('plainText.inspection').toUpperCase()
      case 'insight':
        return t('headers.insight').toUpperCase()
      default:
        return historyItem.assetHistoryType.toUpperCase()
    }
  }

  function getDataSegment(historyItem) {
    switch (historyItem.assetHistoryType) {
      case 'standardTicket':
      case 'scheduledTicket':
        return 'Ticket Selected'
      case 'insight':
        return 'Insight Selected'
      default:
        return undefined
    }
  }

  return (
    <Table items={items} notFound={t('plainText.noAssetFound')}>
      {(historyItems) => (
        <>
          <Head isVisible={false}>
            <Row>
              <Cell />
              <Cell width="1fr" />
              <Cell />
            </Row>
          </Head>
          <Body>
            {historyItems.map((historyItem, i) => (
              <Row
                key={historyItem.id}
                selected={selectedItem === historyItem}
                onClick={() => setSelectedItem(historyItem)}
                onMouseOver={() => {
                  setHoverIndex(i)
                }}
                onMouseOut={() => {
                  setHoverIndex(null)
                }}
                data-segment={getDataSegment(historyItem)}
                data-segment-props={JSON.stringify({
                  type:
                    historyItem.assetHistoryType === 'insight'
                      ? historyItem.type
                      : undefined,
                  priority: priorities.find(
                    (priority) => priority.id === historyItem.priority
                  )?.name,
                  status: historyItem.status,
                  page: 'Asset Details',
                })}
              >
                <Cell className={styles.td}>
                  <Pill>
                    {getHistoryItemType(historyItem, hoverIndex === i)}
                  </Pill>
                </Cell>
                <Cell type="fill" className={styles.titleCell}>
                  {getHistoryItemTitle(historyItem)}
                </Cell>
                <Cell>
                  <Time value={getHistoryItemClosedDate(historyItem)} />
                </Cell>
              </Row>
            ))}
          </Body>
        </>
      )}
    </Table>
  )
}
