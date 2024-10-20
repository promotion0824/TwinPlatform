import { createTheme } from '@mui/material'
import { darkThemePalette } from '@willowinc/palette'
import React from 'react'

function pxToRem(px: number, base = 16) {
  return (1 / base) * px + 'rem'
}

declare module '@mui/material/styles' {
  interface Palette {
    willow: {
      text: {
        highlight: string
        default: string
        muted: string
        subtle: string
      }
      background: {
        base: string
        panel: string
        accent: string
      }
    }
  }
  interface PaletteOptions {
    willow?: {
      text?: {
        highlight?: React.CSSProperties['color']
        default?: React.CSSProperties['color']
        muted?: React.CSSProperties['color']
        subtle?: React.CSSProperties['color']
      }
      background?: {
        base?: React.CSSProperties['color']
        panel?: React.CSSProperties['color']
        accent?: React.CSSProperties['color']
      }
    }
  }
}

const themes = {
  dark: createTheme({
    palette: {
      mode: 'dark',

      // WDS neutral foreground (text and icon) colors
      willow: {
        text: {
          highlight: darkThemePalette.palette.gray[900],
          default: darkThemePalette.palette.gray[800],
          muted: darkThemePalette.palette.gray[500],
          subtle: darkThemePalette.palette.gray[500],
        },
        // WDS neutral background colors
        background: {
          base: darkThemePalette.palette.gray[80],
          panel: darkThemePalette.palette.gray[120],
          accent: darkThemePalette.palette.gray[160],
        },
      },

      // The colors used to style the text.
      text: {
        // The most important text.
        primary: darkThemePalette.palette.gray[900],
        // Secondary text.
        secondary: darkThemePalette.palette.gray[800],
        // Disabled text have even lower visual prominence.
        disabled: darkThemePalette.palette.gray[500],
      },
      // The background colors used to style the surfaces.
      // Consistency between these values is important.
      background: {
        paper: darkThemePalette.palette.gray[80],
        default: darkThemePalette.palette.gray[80],
      },
      // The colors used to style the action elements.
      action: {
        // The color of an active action like an icon button.
        // active: common.white,
        // The color of an hovered action.
        hover: 'rgba(255, 255, 255, 0.08)',
        hoverOpacity: 0.15,
        // The color of a selected action.
        // selected: 'rgba(255, 255, 255, 0.16)',
        // selectedOpacity: 0.16,
        // The color of a disabled action.
        // disabled: 'rgba(255, 255, 255, 0.3)',
        // The background color of a disabled action.
        // disabledBackground: 'rgba(255, 255, 255, 0.12)',
        // disabledOpacity: 0.38,
        // focus: 'rgba(255, 255, 255, 0.12)',
        // focusOpacity: 0.12,
        // activatedOpacity: 0.24,
      },
      // The colors used to represent primary interface elements for a user.
      primary: {
        main: darkThemePalette.palette.purple[400],
        light: darkThemePalette.palette.purple[600],
        dark: darkThemePalette.palette.purple[350],
        contrastText: darkThemePalette.palette.gray[900],
      },
      // The colors used to represent secondary interface elements for a user.
      secondary: {
        main: darkThemePalette.palette.gray[400],
        light: darkThemePalette.palette.gray[600],
        dark: darkThemePalette.palette.gray[350],
        contrastText: darkThemePalette.palette.gray[900],
      },
      // The colors used to represent interface elements that the user should be made aware of.
      error: {
        main: darkThemePalette.palette.red[400],
        light: darkThemePalette.palette.red[600],
        dark: darkThemePalette.palette.red[350],
        contrastText: darkThemePalette.palette.gray[900],
      },
      // The colors used to represent potentially dangerous actions or important messages.
      warning: {
        main: darkThemePalette.palette.orange[400],
        light: darkThemePalette.palette.orange[600],
        dark: darkThemePalette.palette.orange[350],
        contrastText: darkThemePalette.palette.gray[900],
      },
      // The colors used to present information to the user that is neutral and not necessarily important.
      info: {
        main: darkThemePalette.palette.blue[400],
        light: darkThemePalette.palette.blue[600],
        dark: darkThemePalette.palette.blue[350],
        contrastText: darkThemePalette.palette.gray[900],
      },
      // The colors used to indicate the successful completion of an action that user triggered.
      success: {
        main: darkThemePalette.palette.green[400],
        light: darkThemePalette.palette.green[600],
        dark: darkThemePalette.palette.green[350],
        contrastText: darkThemePalette.palette.gray[900],
      },

      // tonalOffset will set primary.light and primary.dark based on primary.main
      // tonalOffset: 0.2,
    },
    typography: (theme) => ({
      htmlFontSize: 16,
      fontSize: 16,
      fontFamily:
        'Poppins, -apple-system, BlinkMacSystemFont, "Segoe UI", Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji"',
      h1: {
        fontSize: pxToRem(20),
        fontWeight: 600,
        color: theme.text.primary,
      },
      h2: {
        fontSize: pxToRem(18),
        fontWeight: 600,
        color: theme.text.primary,
      },
      h3: {
        fontSize: pxToRem(16),
        fontWeight: 600,
        lineHeight: 1.4,
        color: theme.text.primary,
      },
      h4: {
        fontSize: pxToRem(14),
        fontWeight: 600,
        color: theme.text.primary,
      },
      h5: {
        fontWeight: 600,
        fontSize: pxToRem(13),
        color: theme.text.primary,
      },
      h6: {
        fontSize: pxToRem(12),
        fontWeight: 700,
        color: theme.text.primary,
      },
      group: {
        fontSize: pxToRem(11),
        lineHeight: '20px',
        fontWeight: 600,
        textTransform: 'uppercase',
        letterSpacing: '0.1rem',
      },
      body1: {
        fontSize: pxToRem(13),
        color: theme.text.secondary,
        lineHeight: '20px',
      },
      body2: {
        fontSize: pxToRem(12),
        lineHeight: '16px',
        color: theme.text.secondary,
      },
      button: {
        fontSize: pxToRem(13),
        fontWeight: 500,
      },
      caption: {
        fontSize: pxToRem(12),
        textTransform: 'uppercase',
      },
      subtitle1: {
        fontSize: pxToRem(12),
      },
      subtitle2: {
        fontSize: pxToRem(11),
        fontWeight: 400,
      },
      overline: {
        fontSize: pxToRem(12),
        fontWeight: 600,
        textTransform: 'uppercase',
      },
    }),
    components: {
      MuiAppBar: {
        defaultProps: {
          color: 'transparent',
          enableColorOnDark: true,
        },
        styleOverrides: {
          root: ({ theme }) => ({
            backgroundImage: 'none',
            backgroundColor: theme.palette.background.paper,
          }),
        },
      },
      MuiButton: {
        defaultProps: {
          size: 'small',
        },
        styleOverrides: {
          root: {
            textTransform: 'none',
            borderRadius: '2px',
            cursor: 'pointer',
          },
          sizeSmall: {
            fontSize: '0.75rem',
          },
          sizeLarge: {
            fontSize: '0.75rem',
          },
          containedPrimary: ({ theme }) => ({
            '&:hover': {
              backgroundColor: theme.palette.primary.dark,
            },
          }),
          textSecondary: ({ theme }) => ({
            color: theme.palette.text.primary,
          }),
          outlinedPrimary: ({ theme }) => ({
            color: theme.palette.primary.light,
            borderColor: theme.palette.primary.light,
          }),
          outlinedSecondary: ({ theme }) => ({
            color: theme.palette.text.primary,
            borderColor: theme.palette.secondary.light,
          }),
        },
      },
      MuiButtonBase: {
        defaultProps: {
          disableRipple: true,
        },
      },
      MuiCheckbox: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiFormControl: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiInputLabel: {
        defaultProps: {
          shrink: true,
        },
        styleOverrides: {
          root: {
            position: 'relative',
            transform: 'none',
          },
        },
      },
      MuiSelect: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiRadio: {
        defaultProps: {
          size: 'small',
        },
      },
      MuiSwitch: {
        defaultProps: {
          size: 'small',
        },
      },
    },
  }),
}

export default themes
