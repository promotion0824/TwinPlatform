// export everything from MUI
export * from '@mui/x-data-grid-pro'
export { DataGridFooterContainer } from './components'
// export customized overlay component so that user could extend
export { DataGridOverlay } from './components'
// override DataGrid from MUI
export { DataGrid, type DataGridProps } from './DataGrid'
// override DataGridProProps and DataGridPro from MUI so user cannot access them
export const { DataGridProProps, DataGridPro } = {
  DataGridProProps: undefined,
  DataGridPro: undefined,
}
