import React, { HTMLAttributes, forwardRef } from 'react'
import styled from 'styled-components'

import { rem } from '../../utils'

export interface PageTitleItemProps
  extends Omit<HTMLAttributes<HTMLElement>, 'prefix'> {
  href?: string
  /**
   * Signifies this item is the final, current item in the PageTitle.
   * This property is set automatically and cannot be overwritten.
   */
  isCurrent?: boolean
  /**
   * Displayed before the page title item, inside the clickable area.
   * Only displayed on the final item of the PageTitle.
   */
  prefix?: React.ReactNode
  /**
   * Displayed after the page title item, outside the clickable area.
   * Only displayed on the final item of the PageTitle.
   */
  suffix?: React.ReactNode
}

export const PageTitleItem = forwardRef<HTMLLIElement, PageTitleItemProps>(
  ({ children, href, isCurrent, prefix, suffix, ...rest }, ref) => {
    const content = (
      <>
        {isCurrent && prefix && <PrefixContainer>{prefix}</PrefixContainer>}
        {children}
      </>
    )

    return (
      <StyledPageTitleItem $isCurrent={isCurrent} ref={ref} {...rest}>
        {href ? <a href={href}>{content}</a> : content}
        {isCurrent && suffix && <SuffixContainer>{suffix}</SuffixContainer>}
      </StyledPageTitleItem>
    )
  }
)

const StyledPageTitleItem = styled.li<{ $isCurrent?: boolean }>(
  ({ $isCurrent, theme }) => ({
    alignItems: 'center',
    display: 'flex',
    lineHeight: rem(28),

    a: {
      color: theme.color.neutral.fg.default,
      display: 'flex',
      border: '1px solid transparent',
      boxSizing: 'border-box',
      fontWeight: $isCurrent ? 500 : 400,
      textDecoration: 'none',

      '&:focus-visible': {
        border: `1px solid ${theme.color.neutral.fg.default}`,
        borderRadius: theme.radius.r2,
        outline: 'none',
      },

      '&:hover': {
        color: theme.color.neutral.fg.highlight,
      },
    },
  })
)

const PrefixContainer = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  marginRight: theme.spacing.s4,
}))

const SuffixContainer = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  marginLeft: theme.spacing.s8,
}))
