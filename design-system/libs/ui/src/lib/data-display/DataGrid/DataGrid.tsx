import {
  DataGridPro as MuiDataGridPro,
  DataGridProProps as MuiDataGridProProps,
  GridValidRowModel as MuiGridValidRowModel,
} from '@mui/x-data-grid-pro'
import { merge } from 'lodash'
import { ForwardedRef, forwardRef } from 'react'
import { slots as defaultSlots, useSxStyles } from './customStyles'
import { DataGridOverlay } from './components'

// This DataGridProps is currently same as MuiDataGridProProps, but we have
// to repeat the props and descriptions here to enable storybook argsTable
// to pick up them and list them in storybook.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export interface DataGridProps<R extends MuiGridValidRowModel = any>
  extends MuiDataGridProProps<R> {
  columns: MuiDataGridProProps<R>['columns']
  rows: MuiDataGridProProps<R>['rows']
  initialState?: MuiDataGridProProps<R>['initialState']
  /**
   * @example:
   * noRowsOverlay: () => <DataGridOverlay>Customized message</DataGridOverlay>
   * noResultsOverlayMessage: () => <DataGridOverlay>Customized message</DataGridOverlay>
   */
  slots?: MuiDataGridProProps<R>['slots']
  /** @default true */
  disableColumnMenu?: MuiDataGridProProps<R>['disableColumnMenu']
  /**
   * Syntactic sugar for you to customize the display massage when no rows,
   * without need to re-define the `noRowsOverlay` component in `slots` prop.
   */
  noRowsOverlayMessage?: string
  /**
   * Syntactic sugar for you to customize the display massage when no results,
   * without need to re-define the `noResultsOverlay` component in `slots` prop.
   */
  noResultsOverlayMessage?: string

  // sorting
  /** @default "client" */
  sortingMode?: MuiDataGridProProps<R>['sortingMode']
  sortModel?: MuiDataGridProProps<R>['sortModel']
  /**
   * Callback fired when the sort model changes before a column is sorted.
   * @param model — With all properties from [[GridSortModel]].
   * @param details — Additional details for this callback.
   */
  onSortModelChange?: MuiDataGridProProps<R>['onSortModelChange']
  /** @default ['asc', 'desc', null] */
  sortingOrder?: MuiDataGridProProps<R>['sortingOrder']
  /** @default false */
  disableMultipleColumnsSorting?: MuiDataGridProProps<R>['disableMultipleColumnsSorting']

  // selection
  /** @default false */
  checkboxSelection?: MuiDataGridProProps<R>['checkboxSelection']
  rowSelectionModel?: MuiDataGridProProps<R>['rowSelectionModel']
  /**
   * Callback fired when the selection state of one or multiple rows changes.
   * @param rowSelectionModel — With all the row ids [[GridSelectionModel]].
   * @param details — Additional details for this callback.
   */
  onRowSelectionModelChange?: MuiDataGridProProps<R>['onRowSelectionModelChange']
  /** @default false */
  checkboxSelectionVisibleOnly?: MuiDataGridProProps<R>['checkboxSelectionVisibleOnly']
  /** @default false */
  disableMultipleRowSelection?: MuiDataGridProProps<R>['disableMultipleRowSelection']
  /** @default false */
  disableRowSelectionOnClick?: MuiDataGridProProps<R>['disableRowSelectionOnClick']
  /** @default false */
  hideFooterSelectedRowCount?: MuiDataGridProProps<R>['hideFooterSelectedRowCount']
  /**
   * If `false`, the row selection mode is disabled. Which will disable `checkboxSelection`,
   * `rowSelectionOnClick` and `isRowSelectable` related features.
   * @default true
   */
  rowSelection?: MuiDataGridProProps<R>['rowSelection']
  /**
   * Determines if a row can be selected.
   * @param params — With all properties from [[GridRowParams]].
   * @returns — A boolean indicating if the cell is selectable.
   */
  isRowSelectable?: MuiDataGridProProps<R>['isRowSelectable']
  /** @default false */
  keepNonExistentRowsSelected?: MuiDataGridProProps<R>['keepNonExistentRowsSelected']

  // row reordering
  /** @default false */
  rowReordering?: MuiDataGridProProps<R>['rowReordering']
  /**
   * Callback fired when a row is being reordered.
   * @param params — With all properties from [[GridRowOrderChangeParams]].
   * @param event — The event object.
   * @param details — Additional details for this callback.
   */
  onRowOrderChange?: MuiDataGridProProps<R>['onRowOrderChange']

  // column reordering
  /** @default false */
  disableColumnReorder?: MuiDataGridProProps<R>['disableColumnReorder']

  // tree data
  /** @default false */
  treeData?: MuiDataGridProProps<R>['treeData']
  /**
   * Determines the path of a row in the tree data. For instance, a row with the path ["A", "B"] is the child of the row with the path ["A"]. Note that all paths must contain at least one element.
   * @template R
   * @param row — The row from which we want the path.
   * @returns — The path to the row.
   */
  getTreeDataPath?: MuiDataGridProProps<R>['getTreeDataPath']
  /** @default false */
  disableChildrenFiltering?: MuiDataGridProProps<R>['disableChildrenFiltering']
  /** @default false */
  disableChildrenSorting?: MuiDataGridProProps<R>['disableChildrenSorting']
  groupingColDef?: MuiDataGridProProps<R>['groupingColDef']

  // pagination
  // master detail
  // column resizing
  // row grouping
  // filtering
}

/**
 * `DataGrid` is extended from [MUI Data Grid Pro](https://mui.com/x/react-data-grid/).
 *
 * @see TODO: add link to storybook
 */
export const DataGrid = forwardRef(function Table<
  R extends MuiGridValidRowModel
>(
  {
    disableColumnMenu = true,
    rows,
    columns,
    sx: customSx,
    slots: customSlots,
    noRowsOverlayMessage = 'There is no data to display',
    noResultsOverlayMessage = 'No results for your selection',
    ...restProps
  }: DataGridProps<R>,
  ref: ForwardedRef<HTMLTableElement>
) {
  const sx = useSxStyles()
  const overlaySlots = {
    noRowsOverlay: () => (
      <DataGridOverlay>{noRowsOverlayMessage}</DataGridOverlay>
    ),

    noResultsOverlay: () => (
      <DataGridOverlay>{noResultsOverlayMessage}</DataGridOverlay>
    ),
  }

  return (
    <MuiDataGridPro
      ref={ref}
      sx={merge(sx, customSx)}
      slots={merge(defaultSlots, overlaySlots, customSlots)}
      rows={rows}
      columns={columns}
      // we will have below feature(s) default differently to MUI,
      // but won't stop user from changing them
      {...{
        disableColumnMenu,
      }}
      {...restProps}
    />
  )
})
