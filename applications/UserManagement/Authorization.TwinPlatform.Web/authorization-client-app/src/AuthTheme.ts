import { createTheme } from '@mui/material';
import { deepmerge } from "@mui/utils";
import getTheme from "@willowinc/mui-theme";

const willowTheme = getTheme();

// Theme copied from TLM Solution
const AuthDarkTheme = createTheme(deepmerge(willowTheme, {
  components: {
    MuiDialogContentText: {
      styleOverrides: {
        root: {
          paddingBottom:'2em'
        }
      }
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& label': { color: 'grey' },
          '& label.Mui-focused': {
            color: '#42a5f5',
          },
          '& .MuiOutlinedInput-root': {
            '& fieldset': {
              borderColor: 'grey',
            }
          },
          '& .MuiFilledInput-underline:after': {
            borderBottomColor: '#42a5f5',
          },
          '& .MuiFilledInput-root': {
            '&.Mui-focused': {
              borderColor: '#42a5f5',
            },
          },
        },
      },
    },
    MuiListItemText: {
      styleOverrides: {
        root: {
          color: 'white'
        }
      }
    },
    MuiCheckbox: {
      styleOverrides: {
        root: {
          color: 'white',
          '&$checked': {
            color: 'red',
          },
          '& .MuiSvgIcon-root': {
            color: 'white'
          }
        }
      }
    },
    MuiAvatar: {
      styleOverrides: {
        root: {
          color: 'white',
          fontSize: '0.9rem'
        }
      }
    },
    MuiButton: {
      styleOverrides: {
        root: {
          color: 'white'
        }
      }
    }
  },
  palette: {
    mode: 'dark',
    primary: {
      main: '#5340D6', // purple
    },
    secondary: {
      main: '#252525', // dark gray
      contrastText: '#d9d9d9', // light grey
      light: '#616161',
    },
    text: {
      primary: '#d9d9d9',
      secondary: '#959595', // darker light gray
      disabled: '#383838',
    },
    background: {
      paper: '#252525',
      default: '#171717',
    },
    info: {
      main: '#5340D6',
    },
    success: {
      main: '#33ca36', // green
      light: '#47ce4a', // very similar to the main green
      dark: '#0e800f', // darker green
    },
    error: {
      main: '#f44336', // red
      light: '#e57373', // very similar to the main red
      dark: '#d32f2f', // darker red
    },
    action: {
      active: '#d9d9d9',
      activatedOpacity: 1,
      hover: '#5340D6',
      hoverOpacity: 0.3,
      focus: '#5340D6',
      focusOpacity: 0.5,
      selected: '#959595',
      selectedOpacity: 0.5,
    },
  },
  typography: {
    fontFamily: 'Poppins',
    h1: {
      fontFamily: 'Poppins.Semibold',
      fontSize: 22,
    },
    h2: {
      fontFamily: 'Poppins.Medium',
      fontSize: 22,
    },
    h3: {
      fontFamily: 'Poppins.Semibold',
      fontSize: 14,
    },
    h4: {
      fontFamily: 'Poppins.Medium',
      fontSize: 14,
    },
    h5: {
      fontFamily: 'Poppins.Regular',
      fontSize: 14,
    },
    h6: {
      fontFamily: 'Poppins.Semibold',
      fontSize: 14,
    },
    subtitle1: {
      fontFamily: 'Poppins.Medium',
      fontSize: 14,
    },
    subtitle2: {
      fontFamily: 'Poppins.Medium',
      fontSize: 12,
    },
    body1: {
      fontFamily: 'Poppins.Regular',
      fontSize: 14,
    },
    body2: {
      fontFamily: 'Poppins.Semibold',
      fontSize: 12,
    },
    button: {
      fontFamily: 'Poppins',
      fontSize: 12,
      textTransform: 'capitalize',
      secondary: {
        borderSize: '1px',
        borderStyle: 'solid',
        borderColor: '#ffffff',
      },
    },
  },

}));

export default AuthDarkTheme;
