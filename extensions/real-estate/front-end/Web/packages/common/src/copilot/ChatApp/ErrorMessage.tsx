import { useTranslation } from 'react-i18next'
import { titleCase } from '@willow/common'
import { styled } from 'twin.macro'
import { Icon, useTheme, Button } from '@willowinc/ui'
import { useState } from 'react'
import { MsgContainer } from './Styles'

export default function ErrorMessage({
  errorMessage,
}: {
  errorMessage: string
}) {
  const theme = useTheme()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const [isExpanded, setExpanded] = useState(false)

  return (
    <ErrorContainer>
      <div tw="flex w-[100%] gap-[16px] items-start">
        <Icon icon="error" css={{ color: theme.color.core.red.fg.default }} />
        <MsgContent>
          <p>{errorMessage}</p>
          {isExpanded && (
            <p tw="pt-[6px]">{t('plainText.expandedErrorMessage')}</p>
          )}
          <StyledButton
            kind="secondary"
            background="transparent"
            onClick={() => setExpanded(!isExpanded)}
          >
            {titleCase({
              text: isExpanded ? t('headers.showLess') : t('headers.learnMore'),
              language,
            })}
          </StyledButton>
        </MsgContent>
      </div>
    </ErrorContainer>
  )
}

export const ErrorContainer = styled(MsgContainer)(({ theme }) => ({
  background: theme.color.intent.negative.bg.subtle.default,
}))

export const MsgContent = styled.div(({ theme }) => ({
  width: '330px',
  flexShrink: 0,
  color: theme.color.core.red.fg.default,
  ...theme.font.body.lg.regular,
}))

export const StyledButton = styled(Button)(({ theme }) => ({
  color: theme.color.neutral.fg.highlight,
  textDecoration: 'underline',
  padding: 0,
  paddingTop: theme.spacing.s6,

  '&:hover': {
    backgroundColor: 'transparent !important',
  },
}))
