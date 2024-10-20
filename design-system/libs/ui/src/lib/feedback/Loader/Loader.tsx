import {
  Loader as MantineLoader,
  LoaderProps as MantineLoaderProps,
  rem,
} from '@mantine/core'
import { useTheme } from '@willowinc/theme'
import { Sizes } from '../../common'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface LoaderProps
  extends WillowStyleProps,
    Omit<MantineLoaderProps, keyof WillowStyleProps | 'size' | 'color'> {
  /** @default xs  */
  size?: Sizes
  /**
   * Loader appearance
   * @default oval
   */
  variant?: 'oval' | 'bars' | 'dots'
  /**
   * Color intent for Loader
   * @default primary
   */
  intent?: 'primary' | 'secondary' | 'negative' | 'positive'
}

/**
 * `Loader` is an animated spinning icon that lets users know
 * content is being loaded.
 *
 * @see TODO: add link to storybook
 */
// MantineLoader is a SVG and cannot accept ref
export const Loader = ({
  size = 'xs',
  intent = 'primary',
  variant = 'oval',
  ...restProps
}: LoaderProps) => {
  const color = useColors(intent)
  return (
    <MantineLoader
      color={color}
      type={variant}
      size={SIZE_TABLE[size]}
      {...restProps}
      {...useWillowStyleProps(restProps)}
    />
  )
}

const SIZE_TABLE = {
  xs: rem(18),
  sm: rem(22),
  md: rem(36),
  lg: rem(44),
  xl: rem(58),
}

const useColors = (intent: Exclude<LoaderProps['intent'], undefined>) => {
  const theme = useTheme()
  const colorTable = {
    primary: theme.color.intent.primary.border.default,
    secondary: theme.color.intent.secondary.border.default,
    negative: theme.color.intent.negative.border.default,
    positive: theme.color.intent.positive.border.hovered,
  }

  return colorTable[intent]
}
