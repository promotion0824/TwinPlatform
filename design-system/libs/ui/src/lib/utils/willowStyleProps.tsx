import { MantineStyleProps } from '@mantine/core'
import { useTheme } from '@willowinc/theme'
import { includes, isString } from 'lodash'
import { ThemeColorToken, getColor } from './themeColors'
import { SpacingValue, getSpacing } from './themeSpacing'

export interface WillowStyleProps {
  /** margin */
  m?: SpacingValue
  /** margin-top */
  mt?: SpacingValue
  /** margin-bottom */
  mb?: SpacingValue
  /** margin-left */
  ml?: SpacingValue
  /** margin-right */
  mr?: SpacingValue
  /** margin-inline */
  mx?: SpacingValue
  /** margin-block */
  my?: SpacingValue
  /** margin-inline-start */
  ms?: SpacingValue
  /** margin-inline-end */
  me?: SpacingValue

  /** padding */
  p?: SpacingValue
  /** padding-top */
  pt?: SpacingValue
  /** padding-bottom */
  pb?: SpacingValue
  /** padding-left */
  pl?: SpacingValue
  /** padding-right */
  pr?: SpacingValue
  /** padding-inline */
  px?: SpacingValue
  /** padding-block */
  py?: SpacingValue
  /** padding-inline-start */
  ps?: SpacingValue
  /** padding-inline-end */
  pe?: SpacingValue

  /** width */
  w?: SpacingValue
  /** min-width */
  miw?: SpacingValue
  /** max-width */
  maw?: SpacingValue
  /** height */
  h?: SpacingValue
  /** min-height */
  mih?: SpacingValue
  /** max-height */
  mah?: SpacingValue

  /** top */
  top?: SpacingValue
  /** left */
  left?: SpacingValue
  /** bottom */
  bottom?: SpacingValue
  /** right */
  right?: SpacingValue

  /** background */
  bg?: ThemeColorToken
  /** color */
  c?: ThemeColorToken
}

export const spacingProps = [
  'm',
  'mt',
  'mb',
  'ml',
  'mr',
  'mx',
  'my',
  'ms',
  'me',

  'p',
  'pt',
  'pb',
  'pl',
  'pr',
  'px',
  'py',
  'ps',
  'pe',

  'w',
  'miw',
  'maw',
  'h',
  'mih',
  'mah',
  'top',
  'left',
  'bottom',
  'right',
] as const

const isSpacingProp = (prop: string): prop is (typeof spacingProps)[number] => {
  return includes(spacingProps, prop)
}

export const colorProps = ['bg', 'c'] as const

const isColorProp = (prop: string): prop is (typeof colorProps)[number] => {
  return includes(colorProps, prop)
}

export const useWillowStyleProps = (
  props: WillowStyleProps = {}
): MantineStyleProps => {
  const theme = useTheme()

  const spacing: Partial<Record<(typeof spacingProps)[number], SpacingValue>> =
    {}
  const color: Partial<Record<(typeof colorProps)[number], string>> = {}
  const rest: Record<string, string> = {}

  for (const [propName, propValue] of Object.entries(props)) {
    if (isSpacingProp(propName)) {
      spacing[propName] = getSpacing(propValue)
    } else if (isColorProp(propName) && isString(propValue)) {
      color[propName] = getColor(propValue as ThemeColorToken, theme)
    } else {
      rest[propName] = propValue
    }
  }

  return { ...spacing, ...color, ...rest }
}
