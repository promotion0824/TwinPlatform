import { DataGrid } from '@willowinc/ui';
import styled from '@emotion/styled';

export const StyledDataGrid = styled(DataGrid)({
  '&&&': {
    border: '0px',
  },
  '&.MuiDataGrid-root .MuiDataGrid-cell:focus-within': {
    outline: 'none !important',
  },
  '&.MuiDataGrid-root .MuiDataGrid-columnHeader:focus': {
    outline: 'none !important',
  },
  '&.MuiDataGrid-root .MuiDataGrid-columnHeader:focus-within': {
    outline: 'none !important',
  },

  [`& .my-job`]: {
    backgroundColor: ' rgba(89, 69, 215, 0.2) !important;',
  },
});
