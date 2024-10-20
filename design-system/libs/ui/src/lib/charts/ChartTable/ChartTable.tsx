import { forwardRef } from 'react'
import styled from 'styled-components'
import { DataGrid, DataGridProps } from '../../data-display/DataGrid'

const ROW_HEIGHT = 36 as const

const StyledDataGrid = styled(DataGrid)({
  '&, .MuiDataGrid-root': {
    border: 'none',
  },

  '.MuiDataGrid-row--lastVisible div': {
    border: 'none',
  },
})

export interface ChartTableProps extends Omit<DataGridProps, ''> {}

export const ChartTable = forwardRef<HTMLTableElement, DataGridProps>(
  ({ ...restProps }, ref) => {
    return (
      <StyledDataGrid
        columnHeaderHeight={ROW_HEIGHT}
        hideFooter
        rowHeight={ROW_HEIGHT}
        {...restProps}
        ref={ref}
      />
    )
  }
)
