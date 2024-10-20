import type { StoryObj } from '@storybook/react'

import { DataGrid } from '.'
const defaultStory = {
  component: DataGrid,
  title: 'DataGrid',
}

export default defaultStory

type Story = StoryObj<typeof DataGrid>

export const Default: Story = {
  render: () => {
    return <DataGrid columns={columns} rows={rows} />
  },
}

export const ColumnPinning: Story = {
  render: () => {
    return (
      <DataGrid
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

const columns = [
  {
    field: 'id',
    hide: true,
  },
  {
    field: 'desk',
    headerName: 'Desk',
    width: 110,
  },
  {
    field: 'commodity',
    headerName: 'Commodity',
    width: 180,
  },
  {
    field: 'traderName',
    headerName: 'Trader Name',
    width: 120,
  },
  {
    field: 'traderEmail',
    headerName: 'Trader Email',
    width: 150,
  },
  {
    field: 'quantity',
    headerName: 'Quantity',
    type: 'number',
    width: 140,
  },
  {
    field: 'unitPrice',
    headerName: 'Unit Price',
    type: 'number',
    editable: false,
  },
]

const rows = [
  {
    id: '33135287-2c40-5fe1-ad77-92313acb360f',
    desk: 'D-1890',
    commodity: 'Sugar No.14',
    traderName: 'Cole Lopez',
    traderEmail: 'al@hevuggik.net',
    quantity: 16926,
    unitPrice: 26.6,
  },
  {
    id: '7c734bbb-7974-5051-961e-993ea0b325ea',
    desk: 'D-3547',
    commodity: 'Milk',
    traderName: 'Calvin Atkins',
    traderEmail: 'fu@obla.to',
    quantity: 54122,
    unitPrice: 9.65,
  },
  {
    id: '0f63e3e0-2cd2-5d98-af16-a9f18eb59b81',
    desk: 'D-6022',
    commodity: 'Frozen Concentrated Orange Juice',
    traderName: 'Madge Figueroa',
    traderEmail: 'eddofe@wira.fr',
    quantity: 27122,
    unitPrice: 50.9,
  },
  {
    id: 'ffdf9217-ba8c-55d5-9d27-c8f8f89df21a',
    desk: 'D-4915',
    commodity: 'Cocoa',
    traderName: 'Lola Wilson',
    traderEmail: 'gel@budu.mp',
    quantity: 24463,
    unitPrice: 51.91,
  },
  {
    id: '2b5cb0a8-0356-5bba-b430-0e441574242a',
    desk: 'D-9436',
    commodity: 'Rapeseed',
    traderName: 'Jacob Malone',
    traderEmail: 'fis@na.tp',
    quantity: 86648,
    unitPrice: 87.24,
  },
  {
    id: '3675d790-666c-556d-b6e6-0572ab063218',
    desk: 'D-9717',
    commodity: 'Wheat',
    traderName: 'Lina Ryan',
    traderEmail: 'ba@carri.me',
    quantity: 28325,
    unitPrice: 45.25,
  },
]
