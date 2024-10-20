import { Badge, Icon, useTheme } from '@willowinc/ui'
import { noop } from 'lodash'
import { styled } from 'twin.macro'

export default function ChatHeader({
  headerText,
  badgeText,
  isResetDisabled,
  onClose,
  onReset,
}: {
  headerText: string
  badgeText: string
  isResetDisabled: boolean
  onClose: () => void
  onReset: () => void
}) {
  const theme = useTheme()
  return (
    <Container>
      <Title>
        {headerText}
        <Badge variant="outline" color="purple" size="sm">
          {badgeText}
        </Badge>
      </Title>
      <div>
        <Icon
          tw="mr-[16px]"
          icon="delete"
          onClick={isResetDisabled ? noop : onReset}
          style={{
            color: isResetDisabled
              ? theme.color.neutral.fg.muted
              : theme.color.neutral.fg.default,
            cursor: isResetDisabled ? 'default' : 'pointer',
          }}
        />
        <Icon icon="close" onClick={onClose} />
      </div>
    </Container>
  )
}

export const Container = styled.div(({ theme }) => ({
  display: 'flex',
  width: '456px',
  height: '64px',
  padding: theme.spacing.s8,
  paddingLeft: theme.spacing.s16,
  justifyContent: 'space-between',
  alignItems: 'center',
  gap: theme.spacing.s8,
  flexShrink: 0,
  background: theme.color.neutral.bg.base.default,
  color: theme.color.neutral.fg.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  cursor: 'pointer',
}))

export const Title = styled.div(({ theme }) => ({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  gap: theme.spacing.s8,
}))
