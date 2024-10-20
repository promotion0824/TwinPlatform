import { Box } from '@mantine/core'
import React, { cloneElement, forwardRef, HTMLAttributes } from 'react'
import styled, { css } from 'styled-components'
import { IconButton } from '../../buttons/Button'
import { Icon } from '../../misc/Icon'
import { Menu } from '../../overlays/Menu'
import {
  useWillowStyleProps,
  WillowStyleProps,
} from '../../utils/willowStyleProps'

export interface PageTitleProps
  extends WillowStyleProps,
    Omit<HTMLAttributes<HTMLElement>, 'children'> {
  /**
   * The maximum amount of items that will be displayed in the PageTitle component.
   * The overflow menu will be displayed if the number of items exceeds this value,
   * and counts towards the total number of items.
   * @default 3
   */
  maxItems?: number
  children?: React.ReactNode
}

/**
 * `PageTitle` is a breadcrumb navigation component that renders a sequence of links.
 */

export const PageTitle = forwardRef<HTMLDivElement, PageTitleProps>(
  ({ children, maxItems = 3, ...restProps }, ref) => {
    const childrenCount = React.Children.count(children)
    const childrenArray =
      React.Children.map(children, (child, index) => {
        return cloneElement(child as React.ReactElement, {
          isCurrent: index + 1 === childrenCount,
        })
      }) || []

    const renderPageTitle = (child: React.ReactNode, index: number) => (
      <ItemWrapper key={`pagetitle-item-${index}`}>
        <PrimaryItem>{child}</PrimaryItem>
        {index < childrenArray.length - 1 && <Divider />}
      </ItemWrapper>
    )

    const pageTitleWithOverflow = () => {
      // The +1 here is to account for the overflow menu itself
      const overflowMenuLength = childrenArray.length - maxItems + 1

      return (
        <>
          {renderPageTitle(childrenArray[0], 0)}
          <Menu>
            <Menu.Target>
              <IconButton background="transparent" kind="secondary">
                <Icon icon="more_horiz" />
              </IconButton>
            </Menu.Target>
            <Menu.Dropdown>
              {childrenArray
                .slice(1, 1 + overflowMenuLength)
                .map((child, index) => (
                  <Menu.Item key={`menu-item-${index}`}>{child}</Menu.Item>
                ))}
            </Menu.Dropdown>
          </Menu>
          <Divider />
          {childrenArray.slice(1 + overflowMenuLength).map(
            (child, index) =>
              renderPageTitle(child, index + overflowMenuLength + 1) // Preserve original index
          )}
        </>
      )
    }

    return (
      <StyledPageTitle
        ref={ref}
        {...restProps}
        {...useWillowStyleProps(restProps)}
      >
        <ItemWrapper>
          {childrenArray.length > maxItems
            ? pageTitleWithOverflow()
            : childrenArray.map(renderPageTitle)}
        </ItemWrapper>
      </StyledPageTitle>
    )
  }
)

const DividerIcon = styled(Icon)(({ theme }) => ({
  color: theme.color.neutral.fg.muted,
}))

const Divider = () => <DividerIcon icon="chevron_right" size={24} />

const PrimaryItem = styled.div(({ theme }) => ({
  ...theme.font.display.sm.medium,
  // At some point the theme above will be updated to be 20px and the line below can be removed
  fontSize: theme.spacing.s20,
}))

const StyledPageTitle = styled(Box<'nav'>)(
  ({ theme }) => css`
    margin-right: ${theme.spacing.s8};
    display: flex;
    align-items: center;
  `
)

const ItemWrapper = styled.ol`
  list-style: none;
  display: flex;
  align-items: center;
  padding: 0;
  margin: 0;
`
