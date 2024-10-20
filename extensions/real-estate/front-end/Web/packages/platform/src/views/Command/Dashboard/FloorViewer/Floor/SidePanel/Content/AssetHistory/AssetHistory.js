/* eslint-disable react-hooks/exhaustive-deps */
import { useState, useEffect } from 'react'
import { useParams } from 'react-router'
import _ from 'lodash'
import {
  reduceQueryStatuses,
  Progress,
  useDateTime,
  Flex,
  Header,
  Text,
  Select,
  Option,
  DatePicker,
  DatePickerButton,
} from '@willow/ui'
import { useTicketStatuses } from '@willow/common'
import { getDateTimeRange } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions.tsx'
import { useTranslation } from 'react-i18next'
import AssetDetailsModal from 'components/AssetDetailsModal/AssetDetailsModal.tsx'
import { useSite } from 'providers'
import { useQueries } from 'react-query'
import HistoryTable from './HistoryTable'
import {
  fetchAssetInsights,
  FilterOperator,
  statusMap,
} from '../../../../../../../../services/Insight/InsightsService'
import { getTickets } from '../../../../../../../../services/Tickets/TicketsService'
import { getInspections } from '../../../../../../../../services/Inspections/InspectionsServices'

export default function AssetHistory({ assetId }) {
  const dateTime = useDateTime()
  const params = useParams()
  const { t } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const [filter, setFilter] = useState(null)

  function getDefaultDateRange() {
    const defaultQuickRange = '1Y'
    return getDateTimeRange(dateTime.now(), defaultQuickRange)
  }
  const [times, setTimes] = useState(getDefaultDateRange())

  const [response, setResponse] = useState([])
  const [items, setItems] = useState([])
  const [selectedItem, setSelectedItem] = useState()

  const { features: { isInspectionEnabled = false } = {} } = useSite()

  const assetHistoryResponse = useQueries([
    {
      queryKey: ['standardTickets', params.siteId, assetId],
      queryFn: async () => {
        const data = await getTickets({
          siteId: params.siteId,
          assetId,
          isClosed: true,
        })

        return data.map((ticket) => ({
          ...ticket,
          status: ticketStatuses.getByStatusCode(ticket.statusCode)?.status,
          assetHistoryType: 'standardTicket',
        }))
      },
    },
    {
      queryKey: ['scheduledTicket', params.siteId, assetId],
      queryFn: async () => {
        const data = await getTickets({
          siteId: params.siteId,
          assetId,
          isClosed: true,
          scheduled: true,
        })

        return data.map((ticket) => ({
          ...ticket,
          status: ticketStatuses.getByStatusCode(ticket.statusCode)?.status,
          assetHistoryType: 'scheduledTicket',
        }))
      },
    },
    {
      queryKey: ['closedInsights', params.siteId, assetId],
      queryFn: async () => {
        const data = await fetchAssetInsights({
          params: {
            filterSpecifications: [
              {
                field: 'siteId',
                operator: FilterOperator.equalsLiteral,
                value: params.siteId,
              },
              {
                field: 'equipmentId',
                operator: FilterOperator.equalsLiteral,
                value: assetId,
              },
              {
                field: 'status',
                operator: FilterOperator.containedIn,
                value: statusMap.resolved,
              },
            ],
          },
        })

        return data
          .filter((insight) => insight.status === 'closed')
          .map((insight) => ({
            ...insight,
            assetHistoryType: 'insight',
          }))
      },
    },
    {
      queryKey: ['completedInspections', params.siteId, assetId],
      queryFn: async () => {
        const data = isInspectionEnabled
          ? await getInspections(params.siteId)
          : []

        return data
          .filter(
            (inspection) =>
              inspection.checkRecordSummaryStatus === 'completed' &&
              inspection.assetId === assetId
          )
          .map((inspection) => ({
            ...inspection,
            assetHistoryType: 'inspection',
          }))
      },
    },
  ])

  // get status of API calls, eg: loading, success etc....
  const reducedQueryStatus = reduceQueryStatuses(
    assetHistoryResponse.map((query) => query.status)
  )

  useEffect(() => {
    if (reducedQueryStatus === 'success') {
      setResponse([
        ...(assetHistoryResponse?.[0]?.data ?? []), // standardTicket
        ...(assetHistoryResponse?.[1]?.data ?? []), // scheduledTicket
        ...(assetHistoryResponse?.[2]?.data ?? []), // closedInsights
        ...(assetHistoryResponse?.[3]?.data ?? []), // completedInspections
      ])
    }
  }, [
    assetHistoryResponse?.[0].data,
    assetHistoryResponse?.[1].data,
    assetHistoryResponse?.[2].data,
    assetHistoryResponse?.[3].data,
    reducedQueryStatus,
  ])

  useEffect(() => {
    const nextItems = _(response)
      .filter(
        (item) =>
          filter == null ||
          (filter === 'standardTickets' &&
            item.assetHistoryType === 'standardTicket') ||
          (filter === 'scheduledTickets' &&
            item.assetHistoryType === 'scheduledTicket') ||
          (filter === 'insights' && item.assetHistoryType === 'insight') ||
          (filter === 'inspections' && item.assetHistoryType === 'inspection')
      )
      .filter((item) => {
        switch (item.assetHistoryType) {
          case 'standardTicket':
          case 'scheduledTicket':
            if (!item.closedDate) {
              return (
                times[0] <= item.updatedDate && item.updatedDate <= times[1]
              )
            }

            return times[0] <= item.closedDate && item.closedDate <= times[1]
          case 'inspection':
            return (
              times[0] <= item.nextCheckRecordDueTime &&
              item.nextCheckRecordDueTime <= times[1]
            )
          case 'insight':
            return times[0] <= item.updatedDate && item.updatedDate <= times[1]
          default:
            return false
        }
      })
      .orderBy((item) => {
        switch (item.assetHistoryType) {
          case 'standardTicket':
          case 'scheduledTicket':
            return item.closedDate
          case 'inspection':
            return item.submittedDate
          case 'insight':
            return item.updatedDate
          default:
            return item.updatedDate
        }
      }, 'desc')
      .value()

    setItems(nextItems)
  }, [response, filter, times])

  function handleDateRangeChange(nextDateRange) {
    setTimes(nextDateRange.length === 0 ? getDefaultDateRange() : nextDateRange)
  }

  function handleClick(fn) {
    const now = dateTime.now()

    setTimes([fn(now).format(), now.format()])
  }

  return (
    <>
      <Flex fill="content">
        <Header align="middle" padding="small large">
          <Flex
            horizontal
            size="tiny"
            align="middle"
            width="100%"
            fill="content"
          >
            <Text type="h3">{t('plainText.assetHistory')}</Text>
            <Flex
              horizontal
              size="medium"
              align="middle right"
              padding="0 0 0 large"
            >
              <Select
                value={filter}
                placeholder={t('placeholder.all')}
                width="medium"
                onChange={(nextFilter) => setFilter(nextFilter)}
              >
                <Option key="1" value="standardTickets">
                  {t('plainText.standardTickets')}
                </Option>
                <Option key="2" value="scheduledTickets">
                  {t('headers.scheduledTickets')}
                </Option>
                <Option key="3" value="insights">
                  {t('headers.insights')}
                </Option>
                {isInspectionEnabled && (
                  <Option key="4" value="inspections">
                    {t('headers.inspections')}
                  </Option>
                )}
              </Select>
              <DatePicker
                type="date-time-range"
                value={times}
                onChange={handleDateRangeChange}
                data-segment="Asset History Calendar Expanded"
                helper={
                  <>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addHours(-1))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last Hour',
                      })}
                    >
                      {t('plainText.lastHour')}
                    </DatePickerButton>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addHours(-4))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last 4 Hours',
                      })}
                    >
                      {t('plainText.last4Hours')}
                    </DatePickerButton>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addHours(-24))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last 24 Hours',
                      })}
                    >
                      {t('plainText.last24Hours')}
                    </DatePickerButton>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addHours(-48))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last 48 Hours',
                      })}
                    >
                      {t('plainText.last48Hours')}
                    </DatePickerButton>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addDays(-7))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last 7 Days',
                      })}
                    >
                      {t('plainText.last7Days')}
                    </DatePickerButton>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addDays(-30))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last 30 Days',
                      })}
                    >
                      {t('plainText.last30Days')}
                    </DatePickerButton>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addDays(-90))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last 90 Days',
                      })}
                    >
                      {t('plainText.last90Days')}
                    </DatePickerButton>
                    <DatePickerButton
                      onClick={() => handleClick((now) => now.addMonths(-12))}
                      data-segment="Time Series Quick Time Option"
                      data-segment-props={JSON.stringify({
                        option: 'Last 12 Months',
                      })}
                    >
                      {t('plainText.last12Months')}
                    </DatePickerButton>
                  </>
                }
              />
            </Flex>
          </Flex>
        </Header>
        <Flex>
          {reducedQueryStatus === 'loading' ? (
            <Progress />
          ) : (
            <HistoryTable
              items={items}
              selectedItem={selectedItem}
              setSelectedItem={setSelectedItem}
            />
          )}
        </Flex>
      </Flex>
      {selectedItem != null && (
        <AssetDetailsModal
          siteId={selectedItem.siteId}
          item={{
            ...selectedItem,
            modalType: selectedItem.assetHistoryType,
          }}
          onClose={() => setSelectedItem()}
          navigationButtonProps={{
            items,
            selectedItem,
            setSelectedItem,
          }}
          times={times}
          selectedInsightIds={[selectedItem.id]}
        />
      )}
    </>
  )
}
