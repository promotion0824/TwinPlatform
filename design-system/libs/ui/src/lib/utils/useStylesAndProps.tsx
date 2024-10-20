import {
  extractStyleProps,
  STYlE_PROPS_DATA,
  StyleProp,
  SystemPropData,
} from '@mantine/core'
import { keys, mapKeys, merge } from 'lodash'
import { useWillowStyleProps } from './willowStyleProps'

interface ParseStylePropsOptions {
  // Mantine defined this as any
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  styleProps: Record<string, StyleProp<any>>
  data?: Record<string, SystemPropData>
}

function parseStyleProps({
  styleProps,
  data = STYlE_PROPS_DATA,
}: ParseStylePropsOptions) {
  return mapKeys(styleProps, (_, key) => {
    // Note: Mantine uses spacingToken * mantineScale, we decided not to
    // deal with scale yet, so just leave it unapplied until future implementation
    return keys(data).includes(key) ? data[key].property : key
  })
}

/**
 * Used to enable Willow Style Props for our customized components.
 * It will calculate the corresponding styles based on WillowTheme,
 * and apply the styles with `style` attribute. And return with the rest of the props.
 */
export const useStylesAndProps = ({
  style = {},
  ...props
}: Record<string, unknown>) => {
  const { styleProps, rest } = extractStyleProps(useWillowStyleProps(props))

  return {
    // Mantine style props are applied via `style` prop
    style: merge(style, parseStyleProps({ styleProps })),
    ...rest,
  }
}
