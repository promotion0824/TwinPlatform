import tw, { styled, css } from 'twin.macro'
import { Icon, IconName, IconProps } from '@willowinc/ui'

const NotFound = ({
  message,
  icon = 'error',
  size = 24,
}: {
  message: string
  icon?: IconName
  size?: IconProps['size']
}) => (
  <Container>
    <Icon
      icon={icon}
      size={size}
      css={css(({ theme }) => ({ color: theme.color.neutral.fg.muted }))}
    />
    {message}
  </Container>
)

export default NotFound

const Container = styled.div(({ theme }) => ({
  '& > span': {
    fontSize: theme.spacing.s32,
  },
  ...theme.font.heading.md,
  color: theme.color.neutral.fg.default,
  alignItems: 'center',
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
  justifyContent: 'center',
  width: '100%',
}))
