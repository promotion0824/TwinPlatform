import { Button, Icon } from '@willowinc/ui'
import styled from 'styled-components'
import BaseTicketStatusSelect from '../../../TicketStatusSelect/TicketStatusSelect'

export const Container = styled.div(({ theme }) => ({
  width: '483px',
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderRadius: theme.spacing.s2,
  padding: theme.spacing.s24,
  boxShadow: theme.shadow.s3,
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s24,
}))

export const ModalHeader = styled.div(({ theme }) => ({
  ...theme.font.heading.lg,
  color: theme.color.intent.negative.fg.default,
}))

export const Warning = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
  color: theme.color.neutral.fg.default,
}))

export const BaseButton = styled(Button)(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  ...theme.font.body.md,
}))

export const DeleteButton = styled(BaseButton)(({ theme }) => ({
  background: theme.color.intent.negative.bg.bold.default,
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
    background: theme.color.intent.negative.bg.bold.default,
  },
}))

export const ConfirmButton = styled(BaseButton)(({ theme }) => ({
  userSelect: 'none',
  background: theme.color.intent.primary.bg.bold.default,
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
    background: theme.color.intent.primary.bg.bold.default,
  },
}))

export const CancelButton = styled(BaseButton)(({ theme }) => ({
  userSelect: 'none',
  background: 'transparent',
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
    background: 'transparent',
  },
  '&:disabled': {
    background: 'transparent',
  },
}))

export const IndicatorContainer = styled.div(({ theme }) => ({
  display: 'flex',
  color: theme.color.neutral.fg.highlight,
  ...theme.font.body.md,
  lineHeight: theme.spacing.s24,
}))

export const Circle = styled.span<{ $selected: boolean }>(
  ({ theme, $selected }) => ({
    background: $selected
      ? theme.color.intent.primary.bg.bold.default
      : theme.color.intent.secondary.bg.bold.default,
    width: theme.spacing.s24,
    height: theme.spacing.s24,
    borderRadius: '50%',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
  })
)

export const Line = styled.div(({ theme }) => ({
  minWidth: '60px',
  height: '0',
  border: `1px solid ${theme.color.neutral.fg.default}`,
  margin: `${theme.spacing.s12} ${theme.spacing.s8}`,
  flexGrow: 1,
}))

export const HeadingSmall = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
  color: theme.color.neutral.fg.default,
}))

export const HeadingExtraSmall = styled.div(({ theme }) => ({
  ...theme.font.heading.xs,
  color: theme.color.neutral.fg.default,
}))

export const StepContentContainer = styled.div(({ theme }) => ({
  padding: theme.spacing.s16,
  backgroundColor: theme.color.neutral.bg.accent.default,
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  maxHeight: '246px',
  overflowY: 'auto',
}))

const FlexColumn = styled.div({
  height: '30px',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
})

export const TicketLink = styled(FlexColumn)(({ theme }) => ({
  marginRight: theme.spacing.s16,
  cursor: 'pointer',
  ...theme.font.body.sm,
  textDecoration: 'underline',
  color: theme.color.neutral.fg.default,
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
  },
}))

export const StyledIcon = styled(Icon)(({ theme }) => ({
  padding: `${theme.spacing.s2} 0 0 10px`,
  userSelect: 'none',
  cursor: 'pointer',
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
  },
}))

export const stepIndicators = [
  {
    text: 'plainText.closeTickets',
  },
  {
    text: 'plainText.improveYourInsights',
  },
  {
    text: 'plainText.resolveInsight',
    isLast: true,
  },
]

// to not display any heading
const StatusSelectContainer = styled.div({
  '&&& > div': {
    height: '30px',
  },
  '& label': {
    display: 'none',
  },
})

export const TicketStatusSelect = ({
  statusCode,
  onChange,
  disabled,
  className,
}: {
  statusCode: number
  onChange: (code: number) => void
  disabled: boolean
  className?: string
}) => (
  <StatusSelectContainer className={className}>
    <BaseTicketStatusSelect
      onChange={(e) => {
        onChange(e)
      }}
      initialStatusCode={statusCode}
      hideLabel
      isPillSelect={false}
      disabled={disabled}
    />
  </StatusSelectContainer>
)

export const PurpleBorderButton = styled(Button)(({ theme }) => ({
  backgroundColor: 'transparent',
  border: `1px solid ${theme.color.intent.primary.border.default}`,
  color: theme.color.intent.primary.fg.default,
  '&:hover': {
    backgroundColor: 'transparent',
    color: theme.color.intent.primary.fg.hovered,
  },
}))
