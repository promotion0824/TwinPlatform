import { useDemoData } from '@mui/x-data-grid-generator'
import type { Meta, StoryObj } from '@storybook/react'
import { useEffect, useMemo, useState } from 'react'

import {
  DataGrid,
  DataGridProps,
  GridColDef,
  GridRowModel,
  GridRowOrderChangeParams,
  GridRowSelectionModel,
  GridSortModel,
  GridValidRowModel,
  GridValueGetterParams,
  useGridApiRef,
} from '.'
import { Stack } from '../../layout/Stack'
import { Button } from '../../buttons/Button'

const meta: Meta<typeof DataGrid> = {
  title: 'DataGrid',
  component: DataGrid,
  decorators: [
    (Story) => (
      <div css={{ height: 400, width: '100%' }}>
        <Story />
      </div>
    ),
  ],
}
export default meta

type Story = StoryObj<typeof DataGrid>

export const Playground: Story = {
  render: ({ ...args }) => {
    const {
      data: { columns, rows },
    } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 50,
      maxColumns: 6,
    })

    return <DataGrid {...args} rows={rows} columns={columns} />
  },
}
interface NameAdminCellValue {
  name: string
  isAdmin: boolean
}

export const CustomizedColumn: Story = {
  render: ({ ...args }) => {
    const {
      data: { rows, columns: initialColumns, initialState },
    } = useDemoData({
      dataSet: 'Employee',
      visibleFields: ['country', 'dateCreated'],
      rowLength: 10,
    })

    const columns = useMemo<GridColDef[]>(
      () => [
        {
          field: 'nameAdmin',
          headerName: 'Name',
          valueGetter: (params: GridValueGetterParams) => ({
            name: params.row.name,
            isAdmin: params.row.isAdmin,
          }),
          valueFormatter: ({ value }: { value: NameAdminCellValue }) =>
            value.isAdmin ? `${value.name} (admin)` : value.name,
          width: 200,
          sortable: false,
        },
        ...initialColumns,
      ],
      [initialColumns]
    )

    return (
      <DataGrid
        {...args}
        rows={rows}
        columns={columns}
        initialState={{
          columns: initialState?.columns,
          sorting: {
            sortModel: [
              {
                field: 'nameAdmin',
                sort: 'asc',
              },
            ],
          },
        }}
      />
    )
  },
}

export const RowSelection: Story = {
  render: ({ ...args }) => {
    const { data } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 10,
      maxColumns: 6,
    })

    return <DataGrid {...args} {...data} checkboxSelection />
  },
}

export const ControlledRowSelection: Story = {
  render: ({ ...args }) => {
    const { data } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 10,
      maxColumns: 6,
    })

    const [rowSelectionModel, setRowSelectionModel] =
      useState<GridRowSelectionModel>([])

    return (
      <DataGrid
        {...args}
        {...data}
        checkboxSelection
        onRowSelectionModelChange={setRowSelectionModel}
        rowSelectionModel={rowSelectionModel}
      />
    )
  },
}

export const Sorting: Story = {
  render: ({ ...args }) => {
    const { data } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 100,
      maxColumns: 6,
    })

    return <DataGrid {...args} {...data} />
  },
}

export const ControlledSorting: Story = {
  render: ({ ...args }) => {
    const { data } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 100,
      maxColumns: 6,
    })

    const [sortModel, setSortModel] = useState<GridSortModel>([
      {
        field: 'rating',
        sort: 'desc',
      },
    ])

    return (
      <DataGrid
        {...args}
        {...data}
        sortModel={sortModel}
        onSortModelChange={setSortModel}
      />
    )
  },
}

export const RowReordering: Story = {
  render: ({ ...args }) => {
    const {
      data: { rows: initialRows, columns },
    } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 20,
      maxColumns: 6,
    })

    const rows = initialRows.map((row) => ({
      ...row,
      // `__reorder__` renders the content of the dragging row,
      // it could accept a JSX element for example `<div css={{}}>{row['traderName]}</div>`
      __reorder__: row['traderName'],
    }))

    return <DataGrid {...args} rows={rows} columns={columns} rowReordering />
  },
}

export const ColumnReordering: Story = {
  render: ({ ...args }) => {
    const {
      data: { rows, columns },
    } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 20,
      maxColumns: 6,
    })

    return <DataGrid {...args} rows={rows} columns={columns} />
  },
}

function updateRowPositionRequest(
  initialIndex: number,
  newIndex: number,
  rows: Array<GridRowModel>
): Promise<Array<GridRowModel>> {
  return new Promise((resolve) => {
    setTimeout(() => {
      const rowsClone = [...rows]
      const row = rowsClone.splice(initialIndex, 1)[0]
      rowsClone.splice(newIndex, 0, row)
      resolve(rowsClone)
    }, Math.random() * 500 + 100) // simulate network latency
  })
}

export const ControlledRowReordering: Story = {
  render: ({ ...args }) => {
    const { data, loading: initialLoadingState } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 20,
      maxColumns: 6,
    })

    const [rows, setRows] = useState(data.rows)
    const [loading, setLoading] = useState(initialLoadingState)

    useEffect(() => {
      setRows(data.rows)
    }, [data])

    useEffect(() => {
      setLoading(initialLoadingState)
    }, [initialLoadingState])

    const handleRowOrderChange = async ({
      oldIndex,
      targetIndex,
    }: GridRowOrderChangeParams) => {
      setLoading(true)
      const newRows = await updateRowPositionRequest(
        oldIndex,
        targetIndex,
        rows
      )

      setRows(newRows)
      setLoading(false)
    }

    return (
      <DataGrid
        {...args}
        {...data}
        rows={rows}
        rowReordering
        onRowOrderChange={handleRowOrderChange}
        loading={loading}
      />
    )
  },
}

interface Row extends GridValidRowModel {
  hierarchy: Array<string>
  jobTitle: string
  recruitmentDate: Date
  id: number
}

export const TreeData: Story = {
  render: ({ ...args }) => {
    const rows: Row[] = [
      {
        hierarchy: ['Sarah'],
        jobTitle: 'Head of Human Resources',
        recruitmentDate: new Date(2020, 8, 12),
        id: 0,
      },
      {
        hierarchy: ['Thomas'],
        jobTitle: 'Head of Sales',
        recruitmentDate: new Date(2017, 3, 4),
        id: 1,
      },
      {
        hierarchy: ['Thomas', 'Robert'],
        jobTitle: 'Sales Person',
        recruitmentDate: new Date(2020, 11, 20),
        id: 2,
      },
      {
        hierarchy: ['Thomas', 'Karen'],
        jobTitle: 'Sales Person',
        recruitmentDate: new Date(2020, 10, 14),
        id: 3,
      },
      {
        hierarchy: ['Mary'],
        jobTitle: 'Head of Engineering',
        recruitmentDate: new Date(2016, 3, 14),
        id: 8,
      },
      {
        hierarchy: ['Mary', 'Jennifer'],
        jobTitle: 'Tech lead front',
        recruitmentDate: new Date(2016, 5, 17),
        id: 9,
      },
    ]

    const columns: GridColDef[] = [
      { field: 'jobTitle', headerName: 'Job Title', flex: 1 },
      {
        field: 'recruitmentDate',
        headerName: 'Recruitment Date',
        type: 'date',
        flex: 1,
      },
    ]

    // there will be an type error when using `DataGridProps<Row>['getTreeDataPath']`
    const getTreeDataPath: DataGridProps<GridValidRowModel>['getTreeDataPath'] =
      (row) => (row as Row).hierarchy

    return (
      <div style={{ height: 400, width: '100%' }}>
        <DataGrid
          {...args}
          rows={rows}
          columns={columns}
          treeData
          getTreeDataPath={getTreeDataPath}
        />
      </div>
    )
  },
}

export const NoData: Story = {
  render: ({ ...args }) => {
    const {
      data: { columns },
    } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 0,
      maxColumns: 6,
    })

    return <DataGrid {...args} columns={columns} rows={[]} />
  },
}

export const NoFilteredResult: Story = {
  render: ({ ...args }) => {
    const { data } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 10,
      maxColumns: 6,
    })

    return (
      <DataGrid
        {...args}
        {...data}
        initialState={{
          filter: {
            filterModel: {
              items: [{ field: 'quantity', operator: '<', value: '25' }],
            },
          },
        }}
      />
    )
  },
}

// export const Pagination = () => <PaginationTable />

export const Autosizing: Story = {
  render: ({ ...args }) => {
    const apiRef = useGridApiRef()
    const autosizeOptions = {
      includeHeaders: true,
    }
    const {
      data: { columns, rows },
    } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 10,
      maxColumns: 6,
    })

    return (
      <Stack>
        <Button
          onClick={() => {
            apiRef.current.autosizeColumns(autosizeOptions)
          }}
        >
          Autosize all columns
        </Button>

        <div css={{ height: 300, width: '100%' }}>
          <DataGrid
            {...args}
            columns={columns}
            rows={rows}
            apiRef={apiRef}
            autosizeOnMount
            autosizeOptions={autosizeOptions}
          />
        </div>
      </Stack>
    )
  },
}

export const ColumnPinning: Story = {
  render: ({ ...args }) => {
    const {
      data: { columns: originColumns, rows },
    } = useDemoData({
      dataSet: 'Commodity',
      rowLength: 10,
      maxColumns: 6,
    })

    const columns = originColumns.map((column) => {
      if (column.field === 'id') {
        return {
          ...column,
          width: 240,
        }
      }
      if (column.field === 'traderEmail') {
        return {
          ...column,
          width: 220,
        }
      }
      return column
    })

    return (
      <DataGrid
        {...args}
        columns={columns}
        rows={rows}
        initialState={{
          pinnedColumns: {
            left: ['id'],
            right: ['traderEmail'],
          },
        }}
      />
    )
  },
}
