import { createPolymorphicComponent } from '@mantine/core'
import { forwardRef } from 'react'
import styled from 'styled-components'
import {
  UnstyledButton,
  UnstyledButtonProps,
} from '../../buttons/UnstyledButton'
import { Group } from '../../layout/Group'
import { Icon, IconName } from '../../misc/Icon'
import { Tooltip } from '../../overlays/Tooltip'
import { useSidebarContext } from './SidebarContext'

const Container = styled(UnstyledButton<'a'>)<{ $isActive: boolean }>(
  ({ $isActive, theme }) => ({
    backgroundColor: $isActive
      ? theme.color.neutral.bg.accent.default
      : theme.color.neutral.bg.panel.default,
    height: theme.spacing.s32,

    '&:focus-visible': {
      outline: `1px solid ${theme.color.state.focus.border}`,
      outlineOffset: '-1px',
    },

    ':hover': {
      backgroundColor: theme.color.neutral.bg.panel.hovered,
    },
  })
)

const Indicator = styled.div(({ theme }) => ({
  backgroundColor: theme.color.intent.primary.bg.bold.default,
  height: theme.spacing.s32,
  flexShrink: 0,
  width: '3px',
}))

const Label = styled.div(({ theme }) => ({
  ...theme.font.heading.xs,
  color: theme.color.neutral.fg.default,
}))

const LinkIcon = styled(Icon)(({ theme }) => ({
  color: theme.color.neutral.fg.muted,
}))

export interface SidebarLinkProps extends UnstyledButtonProps {
  /**
   * Change the root element to be used for `SidebarLink`.
   * @default 'a'
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  component?: any
  /** Icon to be displayed on the left of the link. */
  icon: IconName
  /**
   * Whether the link is currently active.
   * @default false
   */
  isActive?: boolean
  /** Label to be displayed on the link. */
  label: string
}

export const SidebarLink = createPolymorphicComponent<'a', SidebarLinkProps>(
  forwardRef<HTMLAnchorElement, SidebarLinkProps>(
    ({ component = 'a', icon, isActive = false, label, ...restProps }, ref) => {
      const { isCollapsed } = useSidebarContext()

      return (
        <Group gap={0} wrap="nowrap">
          {isActive && <Indicator />}
          <Container
            component={component}
            $isActive={isActive}
            ml={isActive ? 5 : 's8'}
            mr="s8"
            ref={ref}
            w="100%"
            {...restProps}
          >
            <Group p="s6">
              <Tooltip
                disabled={!isCollapsed}
                label={label}
                offset={8}
                position="right"
                withinPortal
              >
                <LinkIcon icon={icon} />
              </Tooltip>
              {!isCollapsed && <Label>{label}</Label>}
            </Group>
          </Container>
        </Group>
      )
    }
  )
)
