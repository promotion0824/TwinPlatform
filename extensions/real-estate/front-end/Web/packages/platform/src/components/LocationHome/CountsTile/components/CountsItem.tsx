import React, { forwardRef } from 'react'
import { CardProps, Group, Icon, Stack, UnstyledButton } from '@willowinc/ui'
import styled from 'styled-components'
import { ArrowIcon, InteractiveCard } from '../../common'

type Intent = 'negative' | 'notice' | 'positive' | 'primary' | 'secondary'

export interface CountsItemIcon {
  name:
    | 'app_badging'
    | 'circle'
    | 'clock_loader_40'
    | 'release_alert'
    | 'error'
    | 'check_circle'
    | 'new_releases'
  filled?: boolean
}
export interface CountsItemProps extends CardProps {
  /** Label displayed as the heading of the tile. */
  label: string
  /** Main value shown on the tile. */
  value: number
  /** Icon attached to the value. */
  icon?: CountsItemIcon

  /** Color of icon and label */
  intent?: Intent
  /**
   * Function called when tile is clicked.
   * Also causes an arrow icon to be displayed when using a touch device.
   */
  onClick?: () => void
}

const BorderlessInteractiveCard = styled(InteractiveCard)({
  border: 'none',
})

const Label = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))

const PrefixIcon = styled(Icon)<{ $intent: Intent }>(({ $intent, theme }) => ({
  color: theme.color.intent[$intent].fg.default,
}))

const StyledArrowIcon = styled(ArrowIcon)`
  margin-left: auto;
`

const Value = styled.div<{ $intent: Intent }>(({ $intent, theme }) => ({
  ...theme.font.heading.xl2,
  color: theme.color.intent[$intent].fg.default,
}))

const CountsItem = forwardRef<HTMLDivElement, CountsItemProps>(
  ({ label, value, icon, intent = 'primary', onClick, ...restProps }, ref) => (
    <UnstyledButton
      onClick={onClick}
      renderRoot={(props) => (
        <BorderlessInteractiveCard
          $isInteractive={!!onClick}
          background="accent"
          ref={ref}
          {...props}
        />
      )}
      tabIndex={0}
      {...restProps}
    >
      <Stack gap="0" py="s4" px="s6">
        <Group align="flex-start" gap="s4" wrap="nowrap">
          <Label>{label}</Label>
          {onClick && <StyledArrowIcon />}
        </Group>

        <Group align="center" gap="s4" mr="auto" wrap="nowrap">
          {icon && (
            <PrefixIcon
              $intent={intent}
              icon={icon.name}
              filled={icon.filled}
            />
          )}
          <Value $intent={intent}>{value.toLocaleString()}</Value>
        </Group>
      </Stack>
    </UnstyledButton>
  )
)

export default React.memo(CountsItem)
