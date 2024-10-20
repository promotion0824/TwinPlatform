import { Theme } from '@willowinc/theme'
import { get } from 'lodash'

type FlattenObjectKeys<
  T extends Record<string, unknown>,
  Key = keyof T
> = Key extends string
  ? T[Key] extends Record<string, unknown>
    ? `${Key}.${FlattenObjectKeys<T[Key]>}`
    : `${Key}`
  : never

export type ThemeColorToken = FlattenObjectKeys<Theme['color']>

export const getColor = (value: ThemeColorToken, theme: Theme): string => {
  return get(theme.color, value, value)
}
