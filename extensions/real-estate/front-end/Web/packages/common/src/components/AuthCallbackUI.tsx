import { Button, Icon } from '@willowinc/ui'
import { styled, css } from 'twin.macro'
import FullSizeLoader from './FullSizeLoader'
import WillowLogo from './Willow/WillowLogo'

export default function AuthCallbackUI({ hasError }: { hasError: boolean }) {
  return hasError ? (
    <Container>
      <WillowLogo />
      <ErrorIcon icon="warning" size={24} />
      <div>
        An error has occurred trying to sign you in. If the problem persists,
        please contact your site administrator.
      </div>
      <div>
        <Button href="/">Try again</Button>
      </div>
    </Container>
  ) : (
    <FullSizeLoader
      css={css`
        height: 100vh;
      `}
      size="md"
      intent="secondary"
    />
  )
}

const ErrorIcon = styled(Icon)`
  color: ${({ theme }) => theme.color.intent.negative.fg.default};
`

const Container = styled.div`
  height: fit-content;
  margin: auto;
  position: fixed;
  inset: 0;
  display: flex;
  flex-flow: column;
  align-items: center;
  justify-content: center;
  gap: ${({ theme }) => theme.spacing.s16};
  width: 36em;
  text-align: center;
`
