import { createTheme } from "@mui/material";
import type { } from "@mui/x-data-grid-pro/themeAugmentation";
import { deepmerge } from "@mui/utils";
import getTheme from "@willowinc/mui-theme";

const willowTheme = getTheme();

const customTheme = {
  components: {
    MuiAccordionDetails: {
      styleOverrides: {
        root: {
          padding: 1
        },
      },
    },
    MuiAccordionSummary: {
      styleOverrides: {
        root: {
          padding: 0,
          paddingBottom: 1
        },
      },
    },
    MuiDataGrid: {
      styleOverrides: {
        root: {
          '& .MuiDataGrid-cell *': {
            overflow: 'hidden',
            textOverflow: 'ellipsis',
          },
          '.MuiDataGrid-filterIcon': {
            animation: 'fade 2s infinite',
            '@keyframes fade': {
              '0%': { opacity: '1' },
              '50%': { opacity: '0.2' },
              '100%': { opacity: '1' },
            }
          },
          '.MuiDataGrid-columnHeaders': {
            backgroundColor: 'transparent !important'
          },
        }
      }
    },
    //default theme is captilizing which is very unreadable
    MuiFormHelperText: {
      styleOverrides: {
        root: {
          textTransform: 'none'
        }
      }
    },
    //The boxSizing override must be removed once fully on the @willowinc/ui library
    MuiOutlinedInput: {
      defaultProps: {
        notched: false,
      },
      styleOverrides: {
        root: {
          ".MuiOutlinedInput-input": {
            boxSizing: 'content-box !important',
          }
        }
      }
    },
  },
};

const mergedTheme = createTheme(deepmerge(willowTheme, customTheme));

export default mergedTheme;
