import { useMemo } from 'react'
import { useHistory } from 'react-router-dom'
import { useParams } from 'react-router'
import { useTable, HeaderGroup, EnhancedColumn, Row, Cell } from 'react-table'
import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import {
  Tab,
  TabsContent,
  MoreButton,
  MoreDropdownButton,
  Flex,
  NotFound,
  Error,
  Icon,
  Pill,
  Tabs,
  TableComponents,
} from '@willow/ui'
import { Badge } from '@willowinc/ui'
import { styled } from 'twin.macro'
import { ALL_SITES_TAB, OFFLINE_TAB, ONLINE_TAB } from './utils'
import {
  SelectedTab,
  SetSelectedTab,
  PortfolioParam,
  ConnectivityTableData,
  ConnectivityTableState,
} from './types/ConnectivityProvider'

const { Table, THead, TBody, TD, TH, TR } = TableComponents

export default function ConnectivityTable({
  connectivityTableData,
  connectivityTableState,
  selectedTab,
  setSelectedTab,
}: {
  connectivityTableData: ConnectivityTableData
  connectivityTableState: ConnectivityTableState
  selectedTab: SelectedTab
  setSelectedTab: SetSelectedTab
}) {
  const { t } = useTranslation()
  const tabs = [ALL_SITES_TAB, OFFLINE_TAB, ONLINE_TAB]

  const translationMap = {
    [ALL_SITES_TAB]: t('headers.allSites'),
    [OFFLINE_TAB]: t('headers.offline'),
    [ONLINE_TAB]: t('headers.online'),
  } as Record<string, string>

  const handleTabChange = (newTab: string) => {
    if (selectedTab !== newTab) {
      setSelectedTab(newTab)
    }
  }

  const { isLoading, isError, isSuccess } = connectivityTableState

  return (
    <Container>
      <StyledTabs $borderWidth="1px 0 0 1px">
        {tabs.map((tab) => (
          <Tab
            key={tab}
            header={translationMap[tab]}
            selected={selectedTab === tab}
            onClick={() => handleTabChange(tab)}
          />
        ))}
        <TabsContent>
          {isError && <Error />}
          {isLoading && <Loader />}
          {isSuccess && connectivityTableData.length === 0 && <NoConnectors />}
          {isSuccess && connectivityTableData.length > 0 && (
            <ConnectivityTableContent
              connectivityTableData={connectivityTableData}
            />
          )}
        </TabsContent>
      </StyledTabs>
    </Container>
  )
}

const CenteredFlex = styled(Flex)({
  'justify-content': 'center',
  'align-items': 'center',
  marginBottom: 'auto',
  marginTop: 'auto',
})

function Loader() {
  return (
    <CenteredFlex data-testid="loader">
      <Icon icon="progress" />
    </CenteredFlex>
  )
}

function NoConnectors() {
  const { t } = useTranslation()
  return (
    <CenteredFlex>
      <NotFound>{t('plainText.noConnectorsFound')}</NotFound>
    </CenteredFlex>
  )
}
function ConnectivityTableContent({
  connectivityTableData,
}: {
  connectivityTableData: ConnectivityTableData
}) {
  const { t } = useTranslation()
  const { portfolioId } = useParams<PortfolioParam>()
  const history = useHistory()

  function handleActionClick(siteId: string) {
    history.push(`/admin/portfolios/${portfolioId}/sites/${siteId}/connectors`)
  }

  const columns = useMemo(
    () => [
      {
        Header: t('labels.site'),
        id: 'name',
        accessor: 'name',
        Cell: ({ row }) => {
          const site = row.original

          return <SiteCell site={site} />
        },
      },
      {
        Header: t('headers.connectorStatus'),
        id: 'connectorStatus',
        accessor: 'connectorStatus',
        Cell: ({ value, row }) => {
          const site = row.original

          return (
            <Badge
              variant="dot"
              size="md"
              color={site?.color ?? 'gray'}
            >{`${value} ${
              site.isOnline ? t('headers.online') : t('headers.offline')
            }`}</Badge>
          )
        },
      },
      {
        Header: (
          <Flex>
            <div>{t('plainText.dataIn')}</div>
            <div>{`(${_.capitalize(t('plainText.lastHour'))})`}</div>
          </Flex>
        ),
        id: 'dataIn',
        accessor: 'dataIn',
        Cell: ({ value }: { value: number }) => (
          <Pill>{value.toLocaleString()}</Pill>
        ),
      },
      {
        Header: t('plainText.action'),
        id: 'Action',
        accessor: 'Action',
        Cell: ({ row }) => {
          const { siteId } = row.original
          return (
            <>
              <MoreButton data-testid="more-dropdown-button">
                <MoreDropdownButton
                  icon="right"
                  onClick={() => {
                    handleActionClick(siteId)
                  }}
                >
                  {t('plainText.manageConnectors')}
                </MoreDropdownButton>
              </MoreButton>
            </>
          )
        },
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
            <Tr {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map((column: EnhancedColumn, i: number) => (
                <Th $isFirstColumn={i === 0} {...column.getHeaderProps()}>
                  {column.render('Header')}
                </Th>
              ))}
            </Tr>
          ))}
        </THead>
        <TBody {...getTableBodyProps()}>
          {rows.map((row: Row) => {
            prepareRow(row)
            return (
              <Tr
                data-testid="connectivity-table-site-list-row"
                {...row.getRowProps()}
              >
                {row.cells.map((cell: Cell, i: number) => (
                  <Td $isFirstColumn={i === 0} {...cell.getCellProps()}>
                    {cell.render('Cell')}
                  </Td>
                ))}
              </Tr>
            )
          })}
        </TBody>
      </Table>
    </TableContainer>
  )
}

const StyledTabs = styled(Tabs)({
  '>div': {
    '&:before': {
      height: '0.5px !important',
      top: '38px',
    },
  },
})

const TableContainer = styled.div({
  height: '35rem',
  overflow: 'auto',
})

const Container = styled.div({
  height: '100%',
  width: '100%',
  backgroundColor: '#252525',
  'margin-top': 'var(--padding-small)',
})

const Tr = styled(TR)({
  '& td, th': {
    padding: '0 1rem',
  },
  '& td:not(:first-child)': {
    'text-align': 'center',
  },
  '& th:not(:first-child)': {
    'text-align': 'center',
  },
})

const Th = styled(TH)<{ $isFirstColumn: boolean }>((props) => ({
  width: props.$isFirstColumn ? '66%' : 'inherit',
}))

const Td = styled(TD)<{ $isFirstColumn: boolean }>((props) => ({
  width: props.$isFirstColumn ? '66%' : 'inherit',
}))

const StatusRectangle = styled.span<{ $isOnline: boolean }>((props) => ({
  width: '17px',
  height: '6px',
  'border-radius': '3px',
  background: props.$isOnline ? '#33CA36' : '#D9D9D9',
  'margin-top': '13.5px',
  'margin-right': '15px',
}))

function SiteCell({ site }) {
  return (
    <Flex horizontal>
      <StatusRectangle $isOnline={site.isOnline} />
      <Flex>
        <div>{site.name}</div>
        <div>{`${site.city}, ${site.state}, ${site.country}`}</div>
      </Flex>
    </Flex>
  )
}
