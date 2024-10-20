import {
  Flex as MantineFlex,
  FlexProps as MantineFlexProps,
  createPolymorphicComponent,
} from '@mantine/core'
import { forwardRef } from 'react'
import { WillowStyleProps, useWillowStyleProps } from '../../utils'
import { SpacingValue, getSpacing } from '../../utils/themeSpacing'

interface PropsForDocumentation {
  /**
   * `align-items` CSS property.
   * @default 'flex-start'
   */
  align?: React.CSSProperties['alignItems']
  /**
   * `justify-content` CSS property.
   * @default 'flex-start'
   */
  justify?: React.CSSProperties['justifyContent']
  /**
   * `flex-wrap` CSS property.
   * @default 'no-wrap'
   */
  wrap?: React.CSSProperties['flexWrap']
  /**
   * `flex-direction` CSS property.
   * @default 'row'
   */
  direction?: React.CSSProperties['flexDirection']
  /**
   * Change the root element with component prop.
   * @default 'div'
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component?: any // Mantine's component prop is any.
}

interface BaseProps {
  /**
   * Accepts "spacing" properties from the Willow theme (s2, s4, etc.), any valid CSS value,
   * or numbers. Numbers are converted to rem.
   */
  gap?: SpacingValue
  /**
   * `row-gap` CSS property.<br/>
   * Accepts "spacing" properties from the Willow theme (s2, s4, etc.), any valid CSS value,
   * or numbers. Numbers are converted to rem.
   */
  rowGap?: SpacingValue
  /**
   * `column-gap` CSS property.<br/>
   * Accepts "spacing" properties from the Willow theme (s2, s4, etc.), any valid CSS value,
   * or numbers. Numbers are converted to rem.
   */
  columnGap?: SpacingValue
}

export interface FlexProps
  extends BaseProps,
    WillowStyleProps,
    Omit<
      MantineFlexProps,
      keyof WillowStyleProps | 'gap' | 'rowGap' | 'columnGap'
    > {}

/**
 * `Flex` composes elements in a flex container.
 */
export const Flex = createPolymorphicComponent<'div', FlexProps>(
  forwardRef<HTMLDivElement, FlexProps>(
    ({ gap, rowGap, columnGap, ...restProps }, ref) => {
      return (
        <MantineFlex
          gap={getSpacing(gap)}
          rowGap={getSpacing(rowGap)}
          columnGap={getSpacing(columnGap)}
          {...restProps}
          {...useWillowStyleProps(restProps)}
          ref={ref}
        />
      )
    }
  )
)

export const DivForDocumentation = forwardRef<
  HTMLDivElement,
  BaseProps & PropsForDocumentation
>(() => <div />)
