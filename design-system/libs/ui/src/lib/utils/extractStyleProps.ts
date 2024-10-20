import {
  extractStyleProps as mantineExtractStyleProps,
  MantineStyleProps,
} from '@mantine/core'
import { WillowStyleProps } from './willowStyleProps'

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export default function extractStyleProps<T extends Record<string, any>>(
  others: MantineStyleProps & T
): {
  styleProps: WillowStyleProps
  restProps: T
} {
  const { styleProps, rest } = mantineExtractStyleProps(others)

  return {
    styleProps: styleProps as WillowStyleProps,
    restProps: rest,
  }
}
