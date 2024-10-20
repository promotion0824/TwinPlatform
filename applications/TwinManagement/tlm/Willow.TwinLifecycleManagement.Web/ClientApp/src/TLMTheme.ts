import { createTheme } from '@mui/material';
import type {} from '@mui/x-data-grid-pro/themeAugmentation';
import { deepmerge } from '@mui/utils';
import getTheme from '@willowinc/mui-theme';

const willowTheme = getTheme();
const willowThemeComponents = willowTheme!.components;

delete willowThemeComponents!['MuiInputLabel']; // band-aid solution to fix transition animation issues with Mui input label component when using willow custom theme. Removed  style overrides.

const customTheme = {
  components: {
    MuiTablePagination: {
      styleOverrides: {
        root: {
          margin: 'unset !important',
          '& .MuiTablePagination-selectLabel': { margin: 'unset !important' },
          '& .MuiTablePagination-displayedRows': { margin: 'unset !important' },
          '& .MuiTablePagination-input': { fontWeight: 400, fontSize: '0.8rem', lineHeight: '16px' },
          '& .MuiTablePagination-select': { padding: '3px 30px 0 0 !important' },
          '& .MuiTablePagination-selectIcon': { top: 'calc(50% - .5em + -1px) !important' },
        },
      },
    },

    MuiAlert: {
      styleOverrides: {
        root: {
          alignItems: 'center',
        },
      },
    },
    MuiDataGrid: {
      styleOverrides: {
        root: {
          '& .MuiDataGrid-cell:focus': { zIndex: '9 !important' },
          '& .MuiDataGrid-cell:focus-within': { zIndex: '9 !important' },
        },
      },
    },
  },
};

const TLMTheme = createTheme(deepmerge(willowTheme, customTheme));

export default TLMTheme;
