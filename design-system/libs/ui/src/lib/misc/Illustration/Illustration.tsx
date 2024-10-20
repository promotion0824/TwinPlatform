import { forwardRef, HTMLProps } from 'react'

import { ThemeName } from '../../theme'
import {
  useCurrentThemeType,
  useStylesAndProps,
  WillowStyleProps,
} from '../../utils'
import { IllustrationName } from './types'

import noDataDark from './svgs/illustration=no-data, theme=dark.svg'
import noDataLight from './svgs/illustration=no-data, theme=light.svg'
import noPermissionsDark from './svgs/illustration=no-permissions, theme=dark.svg'
import noPermissionsLight from './svgs/illustration=no-permissions, theme=light.svg'
import noResultsDark from './svgs/illustration=no-results, theme=dark.svg'
import noResultsLight from './svgs/illustration=no-results, theme=light.svg'
import notFoundDark from './svgs/illustration=not-found, theme=dark.svg'
import notFoundLight from './svgs/illustration=not-found, theme=light.svg'

export interface IllustrationProps
  extends HTMLProps<HTMLImageElement>,
    WillowStyleProps {
  illustration?: IllustrationName
  /** Will use theme from context if not provided */
  themeName?: ThemeName
}

const illustrations: Record<IllustrationName, Record<ThemeName, string>> = {
  'no-data': { light: noDataLight, dark: noDataDark },
  'no-permissions': { light: noPermissionsLight, dark: noPermissionsDark },
  'no-results': { light: noResultsLight, dark: noResultsDark },
  'not-found': { light: notFoundLight, dark: notFoundDark },
}

/**
 * `Illustration` is an image component renders a list of build-in svgs.
 */
export const Illustration = forwardRef<HTMLImageElement, IllustrationProps>(
  ({ illustration = 'no-permissions', themeName, ...restProps }, ref) => {
    const currentThemeName = useCurrentThemeType()
    const svgTheme = themeName ?? currentThemeName
    const svg = illustrations[illustration][svgTheme]
    return (
      <img
        src={svg}
        alt={`${illustration}-${svgTheme}`}
        ref={ref}
        {...useStylesAndProps(restProps)}
      />
    )
  }
)
