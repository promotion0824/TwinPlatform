import {
  AvatarGroup as MantineAvatarGroup,
  AvatarGroupProps as MantineAvatarGroupProps,
} from '@mantine/core'
import React, { ReactElement, forwardRef } from 'react'
import { WillowStyleProps, useWillowStyleProps } from '../../utils'
import { Avatar } from '../Avatar/Avatar'
import { Box } from '../../misc/Box'
import styled from 'styled-components'
import { css } from 'styled-components'

export interface AvatarGroupProps
  extends WillowStyleProps,
    Omit<MantineAvatarGroupProps, keyof WillowStyleProps> {
  /**
   * The maximum number of avatars to display before truncating.
   */
  maxItems?: number
  /** The avatars to display in the group. */
  children: ReactElement | ReactElement[]
}

const InvalidChildError =
  'AvatarGroup only accepts Avatar components as children.'

const StyledAvatarGroup = styled(MantineAvatarGroup)(
  ({ theme }) => css`
    .mantine-Avatar-root {
      border: 1px solid ${theme.color.neutral.border.default};
      background: none;
    }
  `
)

/**
 * `AvatarGroup` component arranges multiple avatars in an overlapping layout to signify a group of users.
 */
export const AvatarGroup = forwardRef<HTMLDivElement, AvatarGroupProps>(
  ({ maxItems, children, ...restProps }, ref) => {
    const avatarElements = React.Children.map(
      children,
      (child: ReactElement, index: number) => {
        if (child.type === Avatar)
          return React.cloneElement(child, { key: index })
        else throw new Error(InvalidChildError)
      }
    )
    if (maxItems) {
      const extraAvatarElements = avatarElements.splice(maxItems)
      if (extraAvatarElements.length > 0) {
        const tooltip = extraAvatarElements
          .filter((avatar) => avatar.props.tooltip)
          .map((avatar) => <Box>{avatar.props.tooltip}</Box>)

        avatarElements.push(
          <Avatar key="omitted" tooltip={tooltip.length ? tooltip : null}>
            +{extraAvatarElements.length}
          </Avatar>
        )
      }
    }

    return (
      <StyledAvatarGroup
        spacing={8}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        ref={ref}
      >
        {avatarElements}
      </StyledAvatarGroup>
    )
  }
)
