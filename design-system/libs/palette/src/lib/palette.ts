import {
  BackgroundColor,
  Color,
  ContrastColor,
  CssColor,
  InterpolationColorspace,
  Theme,
} from '@adobe/leonardo-contrast-colors'
import config from '../config/palette.json'

export type PaletteTokens = {
  palette: {
    [key: string]: {
      [key: string]: {
        value: string
        type: string
      }
    }
  }
}

type PaletteConfigColor = {
  name: string
  colorspace: InterpolationColorspace
  smooth: boolean
  color: CssColor
}

function createColorOptions({ color, ...rest }: PaletteConfigColor) {
  return new Color({
    ...rest,
    colorKeys: [color],
    ratios: config.ratios,
  })
}

export function generatePalette(lightness: number) {
  const colors = (config.colors as PaletteConfigColor[]).map((configColor) =>
    createColorOptions(configColor)
  )

  const baseColor = new BackgroundColor(
    createColorOptions(config.baseColor as PaletteConfigColor)
  )

  const theme = new Theme({
    lightness,
    contrast: config.contrast,
    saturation: config.saturation,
    backgroundColor: baseColor,
    colors: [baseColor, ...colors],
    output: 'HEX',
  })

  const palette: PaletteTokens = { palette: {} }
  const contrastColors = theme.contrastColors.slice(1) as ContrastColor[]

  contrastColors.forEach(({ name: color, values }) => {
    palette.palette[color] = {}
    values.forEach(({ name, value }) => {
      palette.palette[color][name] = {
        value: value,
        type: 'color',
      }
    })
  })

  return palette
}
