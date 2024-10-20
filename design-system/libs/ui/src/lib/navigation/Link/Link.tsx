import { Anchor, AnchorProps, createPolymorphicComponent } from '@mantine/core'
import { ForwardedRef, forwardRef, ReactNode } from 'react'
import styled from 'styled-components'
import { Group } from '../../layout/Group'
import {
  useWillowStyleProps,
  WillowStyleProps,
} from '../../utils/willowStyleProps'

export type LinkSize = 'xs' | 'sm' | 'md' | 'lg'

export interface LinkProps
  extends WillowStyleProps,
    Omit<AnchorProps, keyof WillowStyleProps | 'size' | 'underline'> {
  /** The contents of the link. */
  children: ReactNode
  /** Displayed before the link. */
  prefix?: ReactNode
  /**
   * The size of the text.
   * @default 'md'
   */
  size?: LinkSize
  /** Displayed after the link. */
  suffix?: ReactNode
}

const StyledAnchor = styled(Anchor)<
  AnchorProps & {
    children: ReactNode
    ref: ForwardedRef<HTMLAnchorElement>
    $size: LinkSize
  }
>(({ $size, theme }) => ({
  ...theme.font.body[$size].regular,
  color: theme.color.intent.primary.fg.default,

  '&:focus-visible': {
    outline: `1px solid ${theme.color.state.focus.border}`,
  },

  '&:hover': {
    color: theme.color.intent.primary.fg.hovered,
  },

  '&:active': {
    color: theme.color.intent.primary.fg.activated,
  },
}))

/** `Link` is used to link to a different page or resource. */
export const Link = createPolymorphicComponent<'a', LinkProps>(
  forwardRef<HTMLAnchorElement, LinkProps>(
    ({ children, prefix, size = 'md', suffix, ...restProps }, ref) => {
      return (
        <StyledAnchor
          $size={size}
          ref={ref}
          underline="never"
          {...restProps}
          {...useWillowStyleProps(restProps)}
        >
          <Group gap="s2">
            {prefix}
            <div style={{ textDecoration: 'underline' }}>{children}</div>
            {suffix}
          </Group>
        </StyledAnchor>
      )
    }
  )
)
