import { createTheme } from '@mui/material'
import palette from '@willowinc/palette'
import * as React from 'react'
import type { } from '@mui/x-data-grid-pro/themeAugmentation';

const willowDarkPalette = palette(10)

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
      },
      status: {
        valid: string
        noTwin: string
        periodOutOfRange: string
        valueOutOfRange: string
        stuck: string
        nonMonotonic: string
        offline: string
      },
      graph: {
        isCapabilityOf: string
        includedIn: string
        locatedIn: string
        manufacturedBy: string
        isFedBy: string
        isPartOf: string
        comprises: string
        feeds: string
        feedsACElec: string
        feedsWater: string
        feedsSprinklerWater: string
        feedsCondenserWater: string
        feedsCondensate: string
        feedsMakeupWater: string
        feedsSteam: string
        feedsIrrigationWater: string
        feedsStormDrainage: string
        feedsChilledWater: string
        feedsRefrig: string
        feedsColdDomesticWater: string
        feedsHotWater: string
        feedsSupplyAir: string
        feedsReturnAir: string
        feedsOutsideAir: string
        feedsAir: string
        feedsMech: string
        feedsDriveElec: string
        feedsGas: string
        feedsFuelOil: string
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
      status?: {
        valid?: React.CSSProperties['color']
        noTwin?: React.CSSProperties['color']
        periodOutOfRange?: React.CSSProperties['color']
        valueOutOfRange?: React.CSSProperties['color']
        stuck?: React.CSSProperties['color']
        nonMonotonic?: React.CSSProperties['color']
        offline?: React.CSSProperties['color']
      }
      graph?: {
        isCapabilityOf?: React.CSSProperties['color']
        includedIn?: React.CSSProperties['color']
        locatedIn?: React.CSSProperties['color']
        manufacturedBy?: React.CSSProperties['color']
        isFedBy?: React.CSSProperties['color']
        isPartOf?: React.CSSProperties['color']
        comprises?: React.CSSProperties['color']
        feeds?: React.CSSProperties['color']
        feedsACElec?: React.CSSProperties['color']
        feedsWater?: React.CSSProperties['color']
        feedsSprinklerWater?: React.CSSProperties['color']
        feedsCondenserWater?: React.CSSProperties['color']
        feedsCondensate?: React.CSSProperties['color']
        feedsMakeupWater?: React.CSSProperties['color']
        feedsSteam?: React.CSSProperties['color']
        feedsIrrigationWater?: React.CSSProperties['color']
        feedsStormDrainage?: React.CSSProperties['color']
        feedsChilledWater?: React.CSSProperties['color']
        feedsRefrig?: React.CSSProperties['color']
        feedsColdDomesticWater?: React.CSSProperties['color']
        feedsHotWater?: React.CSSProperties['color']
        feedsSupplyAir?: React.CSSProperties['color']
        feedsReturnAir?: React.CSSProperties['color']
        feedsOutsideAir?: React.CSSProperties['color']
        feedsAir?: React.CSSProperties['color']
        feedsMech?: React.CSSProperties['color']
        feedsDriveElec?: React.CSSProperties['color']
        feedsGas?: React.CSSProperties['color']
        feedsFuelOil?: React.CSSProperties['color']
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
          highlight: willowDarkPalette['gray-900'],
          default: willowDarkPalette['gray-800'],
          muted: willowDarkPalette['gray-500'],
          subtle: willowDarkPalette['gray-500'],
        },
        // WDS neutral background colors
        background: {
          base: willowDarkPalette['gray-80'],
          panel: willowDarkPalette['gray-120'],
          accent: willowDarkPalette['gray-160'],
        },
        //Time Series Status colors
        status: {
          valid: willowDarkPalette['green-600'], //green
          noTwin: willowDarkPalette['purple-600'], //purple
          periodOutOfRange: willowDarkPalette['red-400'], //red
          valueOutOfRange: willowDarkPalette['orange-600'], //orange
          stuck: willowDarkPalette['blue-500'], //blue
          nonMonotonic: willowDarkPalette['pink-500'], //pink
          offline: willowDarkPalette['gray-600'], //grey
        },
        //Meta Graph Relationship colors
        graph: {
          isFedBy: willowDarkPalette['blue-600'], //blue
          comprises: willowDarkPalette['blue-400'], //blue
          feedsWater: willowDarkPalette['blue-500'], //blue
          feedsCondenserWater: willowDarkPalette['blue-500'], //blue
          feedsCondensate: willowDarkPalette['blue-500'], //blue
          feedsMakeupWater: willowDarkPalette['blue-500'], //blue
          feedsIrrigationWater: willowDarkPalette['blue-500'], //blue
          feedsStormDrainage: willowDarkPalette['blue-500'], //blue
          feedsChilledWater: willowDarkPalette['blue-500'], //blue
          feedsRefrig: willowDarkPalette['blue-500'], //blue
          feedsColdDomesticWater: willowDarkPalette['blue-500'], //blue

          feedsMech: willowDarkPalette['orange-225'], //brown
          feedsDriveElec: willowDarkPalette['orange-225'], //brown

          feedsSupplyAir: willowDarkPalette['cyan-600'], //cyan
          feedsReturnAir: willowDarkPalette['cyan-600'], //cyan
          feedsOutsideAir: willowDarkPalette['cyan-600'], //cyan
          feedsAir: willowDarkPalette['cyan-600'], //cyan

          isPartOf: willowDarkPalette['green-600'], //green

          feeds: willowDarkPalette['gray-500'], //gray
          feedsSteam: willowDarkPalette['gray-500'], //gray

          isCapabilityOf: willowDarkPalette['orange-600'], //orange
          feedsACElec: willowDarkPalette['orange-500'], //orange

          locatedIn: willowDarkPalette['purple-600'], //purple

          feedsHotWater: willowDarkPalette['red-400'], //red

          includedIn: willowDarkPalette['cyan-600'], //turquoise
          feedsSprinklerWater: willowDarkPalette['cyan-500'], //turquoise

          manufacturedBy: willowDarkPalette['yellow-600'], //yellow
          feedsGas: willowDarkPalette['yellow-500'], //yellow
          feedsFuelOil: willowDarkPalette['yellow-500'], //yellow
        }
      },

      // The colors used to style the text.
      text: {
        // The most important text.
        primary: willowDarkPalette['gray-900'],
        // Secondary text.
        secondary: willowDarkPalette['gray-800'],
        // Disabled text have even lower visual prominence.
        disabled: willowDarkPalette['gray-500'],
      },
      // The background colors used to style the surfaces.
      // Consistency between these values is important.
      background: {
        paper: willowDarkPalette['gray-80'],
        default: willowDarkPalette['gray-80'],
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
        main: willowDarkPalette['purple-400'],
        light: willowDarkPalette['purple-600'],
        dark: willowDarkPalette['purple-350'],
        contrastText: willowDarkPalette['gray-900'],
      },
      // The colors used to represent secondary interface elements for a user.
      secondary: {
        main: willowDarkPalette['gray-400'],
        light: willowDarkPalette['gray-600'],
        dark: willowDarkPalette['gray-350'],
        contrastText: willowDarkPalette['gray-900'],
      },
      // The colors used to represent interface elements that the user should be made aware of.
      error: {
        main: willowDarkPalette['red-400'],
        light: willowDarkPalette['red-600'],
        dark: willowDarkPalette['red-350'],
        contrastText: willowDarkPalette['gray-900'],
      },
      // The colors used to represent potentially dangerous actions or important messages.
      warning: {
        main: willowDarkPalette['orange-400'],
        light: willowDarkPalette['orange-600'],
        dark: willowDarkPalette['orange-350'],
        contrastText: willowDarkPalette['gray-900'],
      },
      // The colors used to present information to the user that is neutral and not necessarily important.
      info: {
        main: willowDarkPalette['blue-400'],
        light: willowDarkPalette['blue-600'],
        dark: willowDarkPalette['blue-350'],
        contrastText: willowDarkPalette['gray-900'],
      },
      // The colors used to indicate the successful completion of an action that user triggered.
      success: {
        main: willowDarkPalette['green-400'],
        light: willowDarkPalette['green-600'],
        dark: willowDarkPalette['green-350'],
        contrastText: willowDarkPalette['gray-900'],
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
          shrink: false,
        },
        styleOverrides: {
          root: {
            position: 'relative',
            transform: 'none',
          },
        },
      },
      MuiOutlinedInput: {
        defaultProps: {
          notched: false,
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
      //for mui grid filter and sort buttons otherwise they are really difficult to spot
      MuiIconButton: {
        styleOverrides: {
          root: {
            color: 'white',
            ".MuiDataGrid-filterIcon": {
              animation: 'fade 2s infinite',
              '@keyframes fade': {
                '0%': { opacity: '1' },
                '50%': { opacity: '0.2' },
                '100%': { opacity: '1' },
              }
            }
          }
        }
      },
      MuiDataGrid: {
        styleOverrides: {
          root: {
            '& .MuiDataGrid-cell *': {
              overflow: 'hidden',
              textOverflow: 'ellipsis',
            },
          }
        }
      },
      MuiAccordionSummary: {
        styleOverrides: {
          root: {
            padding: 0
          },
        },
      },
      MuiTab: {
        styleOverrides: {
          root: ({ theme }) => ({
            backgroundColor: theme.palette.background.paper,
            color: theme.palette.grey[600],
            paddingBottom: theme.spacing(0),
            '&.Mui-selected': {
              color: theme.palette.primary
            }
          })
        }
      }
    }
  }),
}

export default themes
