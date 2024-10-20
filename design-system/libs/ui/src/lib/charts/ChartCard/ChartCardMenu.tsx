import * as echarts from 'echarts'
import xlsx from 'node-xlsx'
import Papa from 'papaparse'
import { useMemo } from 'react'
import styled from 'styled-components'
import { v4 as uuidv4 } from 'uuid'

import { Button, IconButton } from '../../buttons/Button'
import {
  DataGrid,
  GridColDef,
  GridValidRowModel,
} from '../../data-display/DataGrid'
import { useDisclosure } from '../../hooks'
import { Icon } from '../../misc/Icon'
import { Menu } from '../../overlays/Menu'
import { Modal } from '../../overlays/Modal'
import { downloadFile } from '../utils'

const ROW_HEIGHT = 36 as const

type DownloadFormat = 'jpeg' | 'png'

type ChartDataRow = (number | string)[]

interface ChartCardMenuProps {
  data?: Record<string, number | string>[]
  echartsInstance?: echarts.ECharts
  title: string
}

function convertDataToChartDataRows(
  data: Record<string, number | string>[]
): ChartDataRow[] {
  return [Object.keys(data[0]), ...data.map((row) => Object.values(row))]
}

function getChartData(echartsInstance: echarts.ECharts): ChartDataRow[] {
  const dataset = echartsInstance.getOption()['dataset']
  if (!Array.isArray(dataset) || !dataset.length) return []
  return dataset[0].source
}

const StyledDataGrid = styled(DataGrid)({
  border: 'none',
})

const ChartDataGrid = ({ data }: { data: ChartDataRow[] }) => {
  const columns: GridColDef[] = useMemo(
    () =>
      data[0].map((column) => ({
        field: column.toString(),
      })),
    [data]
  )

  const rows = useMemo(
    () =>
      data.slice(1).map((row: Array<string | number>) => {
        const rowObject: GridValidRowModel = { id: uuidv4() }

        columns.forEach((column, index) => {
          rowObject[column.field] = row[index]
        })

        return rowObject
      }),
    [columns, data]
  )

  return (
    <StyledDataGrid
      columnHeaderHeight={ROW_HEIGHT}
      columns={columns}
      hideFooter
      rowHeight={ROW_HEIGHT}
      rows={rows}
    />
  )
}

const ModalFooter = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s8,
  justifyContent: 'flex-end',
  padding: theme.spacing.s8,
}))

/**
 * ChartCardMenu requires either a data array or an ECharts instance.
 */
export const ChartCardMenu = ({
  data,
  echartsInstance,
  title,
}: ChartCardMenuProps) => {
  const [modalOpened, { open: openModal, close: closeModal }] =
    useDisclosure(false)

  const chartCardData = echartsInstance
    ? getChartData(echartsInstance)
    : data
    ? convertDataToChartDataRows(data)
    : []

  const downloadChartImage = (
    echartsInstance: echarts.ECharts,
    format: DownloadFormat = 'png'
  ) => {
    const filename = `${title}.${format}`
    const dataUrl = echartsInstance.getDataURL({
      type: format,
    })
    downloadFile(filename, dataUrl)
  }

  const downloadCsv = (data: ChartDataRow[]) => {
    const filename = `${title}.csv`
    const csv = Papa.unparse(data)
    const dataUrl = `data:text/csv;charset=utf-8,${csv}`
    downloadFile(filename, dataUrl)
  }

  const downloadXlsx = (data: ChartDataRow[]) => {
    const filename = `${title}.xlsx`
    const xlsxData = xlsx.build([{ name: title, data, options: {} }])
    const blob = new Blob([Uint8Array.from(xlsxData).buffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    })
    const dataUrl = URL.createObjectURL(blob)
    downloadFile(filename, dataUrl)
  }

  return (
    <>
      <Menu>
        <Menu.Target>
          <IconButton
            background="transparent"
            icon="more_vert"
            kind="secondary"
          />
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Item
            onClick={() => openModal()}
            prefix={<Icon icon="view_list" />}
          >
            View Data
          </Menu.Item>

          {echartsInstance && (
            <Menu.Item
              onClick={() => downloadChartImage(echartsInstance)}
              prefix={<Icon icon="download" />}
            >
              Download Image
            </Menu.Item>
          )}

          <Menu.Item
            onClick={() => downloadXlsx(chartCardData)}
            prefix={<Icon icon="table" />}
          >
            Download Excel
          </Menu.Item>
          <Menu.Item
            onClick={() => downloadCsv(chartCardData)}
            prefix={<Icon icon="download" />}
          >
            Download CSV
          </Menu.Item>
        </Menu.Dropdown>
      </Menu>

      <Modal header={title} opened={modalOpened} onClose={closeModal} size="xl">
        <ChartDataGrid data={chartCardData} />
        <ModalFooter>
          <Button
            kind="secondary"
            onClick={() => downloadXlsx(chartCardData)}
            prefix={<Icon icon="table" />}
          >
            Download Excel
          </Button>
          <Button
            kind="secondary"
            onClick={() => downloadCsv(chartCardData)}
            prefix={<Icon icon="download" />}
          >
            Download CSV
          </Button>
        </ModalFooter>
      </Modal>
    </>
  )
}
