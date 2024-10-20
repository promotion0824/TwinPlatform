/* eslint-disable consistent-return */
import { useMemo } from 'react'
import { Badge } from '@willowinc/ui'
import { useTable, HeaderGroup, EnhancedColumn, Row, Cell } from 'react-table'
import { useTranslation, TFunction } from 'react-i18next'
import { Error, Flex } from '@willow/ui'
import { styled } from 'twin.macro'
import _ from 'lodash'
import {
  TableContainer,
  Loader,
  NoResults,
  Table,
  TBody,
  THead,
  TH,
} from '../styled-components'
import {
  ENABLED,
  ONLINE,
  DISRUPTED,
  READY,
  DISABLED,
  OFFLINE,
  ARCHIVED,
} from '../../../../Connectivity/utils'
import { ConnectivityTableDataType } from '../types'
import {
  ConnectorStat,
  Telemetries,
} from '../../../../../../../services/Connectors/ConnectorsService'
import { useManageConnectors } from '../providers/ManageConnectorsProvider'

export default function Connectors() {
  const { t } = useTranslation()

  const { connectorsStatsQuery, setConnectorId } = useManageConnectors()

  const {
    data: connectorsTableData = [],
    isLoading,
    isSuccess,
    isError,
    isRefetching,
  } = connectorsStatsQuery

  return (
    <>
      {isError && <Error />}
      {(isLoading || isRefetching) && <Loader />}
      {isSuccess && connectorsTableData.length === 0 && (
        <NoResults notFound={t('plainText.noConnectorsFound')} />
      )}
      {isSuccess && connectorsTableData.length > 0 && (
        <ConnectorsTableContent
          connectivityTableData={connectorsTableData}
          onSelectedConnectorClick={(connector: ConnectorStat) => {
            setConnectorId(connector.connectorId)
          }}
        />
      )}
    </>
  )
}

function ConnectorsTableContent({
  connectivityTableData,
  onSelectedConnectorClick,
}: {
  connectivityTableData: ConnectivityTableDataType
  onSelectedConnectorClick: (connector: ConnectorStat) => void
}) {
  const { t } = useTranslation()

  const columns = useMemo(
    () => [
      {
        Header: t('headers.connectors'),
        id: 'connectorId',
        accessor: 'connectorId',
        Cell: ({ row }: { row: Row }) => {
          const connector = row.original

          return <ConnectorCell connector={connector} />
        },
      },
      {
        Header: t('headers.connectorStatus'),
        id: 'currentStatus',
        accessor: 'currentStatus',
        Cell: ({ value }: { value: string }) => (
          <Badge size="md" variant="dot" color={connectorStatusColorMap[value]}>
            {connectionStatusTranslationMap(value, t)}
          </Badge>
        ),
      },
      {
        Header: t('plainText.switch'),
        id: 'currentSetState',
        accessor: 'currentSetState',
        Cell: ({ value }: { value: string }) => (
          <Badge
            size="md"
            variant="dot"
            color={value === ENABLED ? 'green' : 'gray'}
          >
            {switchTranslationMap(value, t)}
          </Badge>
        ),
      },
      {
        Header: (
          <Flex>
            <div>{t('plainText.dataIn')}</div>
            <div>{`(${_.capitalize(t('plainText.lastHour'))})`}</div>
          </Flex>
        ),
        id: 'telemetry',
        accessor: 'telemetry',
        Cell: ({ value }: { value: Telemetries }) => (
          // get data point in the last hour
          <DataPoints
            dataPoints={value.length > 0 ? value[0].totalTelemetryCount : 0}
          />
        ),
      },
    ],
    [connectivityTableData]
  )

  const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } =
    useTable({
      columns,
      data: connectivityTableData,
    })

  return (
    <TableContainer>
      <Table {...getTableProps()}>
        <THead>
          {headerGroups.map((headerGroup: HeaderGroup) => (
            <TR {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map((column: EnhancedColumn) => (
                <TH {...column.getHeaderProps()}>{column.render('Header')}</TH>
              ))}
            </TR>
          ))}
        </THead>
        <TBody {...getTableBodyProps()}>
          {rows.map((row: Row) => {
            prepareRow(row)
            return (
              <TR
                {...row.getRowProps()}
                onClick={() => {
                  onSelectedConnectorClick(row.original)
                }}
              >
                {row.cells.map((cell: Cell, i: number) => (
                  <TD $isFirstColumn={i === 0} {...cell.getCellProps()}>
                    {cell.render('Cell')}
                  </TD>
                ))}
              </TR>
            )
          })}
        </TBody>
      </Table>
    </TableContainer>
  )
}
const TR = styled.tr({
  height: '47px',
  verticalAlign: 'center',
  '& td, th': {
    padding: '0 1rem',
  },
  '& td:not(:first-child)': {
    'text-align': 'center',
  },
  '& th:not(:first-child)': {
    'text-align': 'center',
  },
  '&:hover': {
    backgroundColor: '#2B2B2B !important',
    cursor: 'pointer !important',
  },
})

const TD = styled.td<{ $isFirstColumn: boolean }>((props) => ({
  borderBottom: '1px solid #383838',
  width: props.$isFirstColumn ? '70%' : 'inherit',
}))

function connectionStatusTranslationMap(
  connectionStatus: string,
  t: TFunction
) {
  switch (connectionStatus) {
    case ONLINE:
      return t('headers.online')
    case DISRUPTED:
      return t('plainText.disrupted')
    case READY:
      return t('plainText.ready')
    case OFFLINE:
      return t('headers.offline')
    case DISABLED:
      return t('plainText.disabled')
    case ARCHIVED:
      return t('headers.archived')
    default:
      return connectionStatus
  }
}
function switchTranslationMap(currentSetState: string, t: TFunction) {
  switch (currentSetState) {
    case ENABLED:
      return t('plainText.enabled')
    case DISABLED:
      return t('plainText.disabled')
    case ARCHIVED:
      return t('headers.archived')
    default:
      return currentSetState
  }
}

const StatusRectangle = styled.span<{ status?: string }>(
  ({ theme, status }) => {
    const backgroundColor =
      status != null
        ? connectorStatusColorMap[status] === 'red'
          ? theme.color.intent.negative.fg.hovered
          : theme.color.core[connectorStatusColorMap[status]].fg.default
        : theme.color.intent.secondary.fg.default

    return {
      width: '17px',
      height: '6px',
      'border-radius': '3px',
      'margin-top': '5px',
      'margin-right': '15px',
      background: backgroundColor,
    }
  }
)

function ConnectorCell({ connector }: { connector: ConnectorStat }) {
  return (
    <Flex horizontal>
      <StatusRectangle status={connector.currentStatus} />
      <Flex>
        <div>{connector.connectorName}</div>
      </Flex>
    </Flex>
  )
}

const connectorStatusColorMap = {
  DISABLED: 'gray',
  ONLINE: 'green',
  ENABLED: 'green',
  DISRUPTED: 'orange',
  READY: 'purple',
  OFFLINE: 'red',
  ARCHIVED: 'gray',
}

export function DataPoints({ dataPoints }: { dataPoints: number }) {
  const { t } = useTranslation()
  const isOnline = dataPoints > 0
  return (
    <Badge variant="dot" size="md" color={isOnline ? 'green' : 'red'}>
      {isOnline ? (
        <>{`${dataPoints.toLocaleString()} ${t('plainText.points')}`}</>
      ) : (
        t('headers.offline')
      )}
    </Badge>
  )
}
