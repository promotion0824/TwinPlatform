import { titleCase } from '@willow/common'
import { useAnalytics, useUser } from '@willow/ui'
import { Icon, IconName, Tooltip } from '@willowinc/ui'
import { useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import { Message, useChat } from './ChatContext'

export default function MessageFooter({
  message,
  messageContent,
  onSetShowCitation,
  isCitations,
  showCitation,
}: {
  messageContent: string
  onSetShowCitation: () => void
  message: Message
  isCitations: boolean
  showCitation: boolean
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const user = useUser()
  const analytics = useAnalytics()
  const { copilotSessionId } = useChat()

  const [clickedIcon, setClickedIcon] = useState('')
  const timerRef = useRef<NodeJS.Timeout | null>(null)

  const handleClick = (clickState) => {
    if (clickedIcon !== clickState) {
      setClickedIcon(clickState)
    }
    if (timerRef.current) {
      clearTimeout(timerRef.current)
    }
    timerRef.current = setTimeout(() => {
      setClickedIcon('')
    }, 2000)
  }

  return (
    <IconsContainer>
      {[
        {
          description: message?.id
            ? showCitation
              ? t('plainText.hideCitations')
              : t('plainText.showCitations')
            : '',
          onClick: () => {
            onSetShowCitation()
          },
          isIcon: false,
          isVisible: isCitations,
        },
        {
          iconName: 'content_copy',
          description: t('labels.copy'),
          onClick: () => {
            analytics.track('Copilot - Answer Copied', {
              sessionID: copilotSessionId,
              userEmail: user.email,
            })
            navigator.clipboard.writeText(messageContent)
          },
          isIcon: true,
          isVisible: true,
        },
      ].map(({ iconName, description, onClick, isIcon, isVisible }) => (
        <div
          style={{
            width: description === t('labels.citations') ? '100%' : undefined,
          }}
          key={iconName}
        >
          {isIcon ? (
            <IconToolTip
              name={
                (clickedIcon === iconName
                  ? 'check_circle'
                  : iconName) as IconName
              }
              description={description}
              clicked={clickedIcon === iconName}
              onClick={() => {
                handleClick(iconName)
                onClick?.()
              }}
              language={language}
            />
          ) : (
            isVisible && (
              <ReadMore onClick={onClick}>
                {titleCase({ text: description, language })}
              </ReadMore>
            )
          )}
        </div>
      ))}
    </IconsContainer>
  )
}

const IconToolTip = ({
  name,
  description,
  clicked,
  onClick,
  language,
}: {
  name: IconName
  description: string
  clicked: boolean
  onClick: () => void
  language: string
}) => (
  <Tooltip
    label={titleCase({ text: description, language })}
    withArrow
    disabled={clicked}
  >
    <Icon
      tw="cursor-pointer"
      icon={name}
      onClick={onClick}
      css={css(({ theme }) => ({
        color: clicked ? theme.color.intent.positive.fg.default : 'inherit',
      }))}
    />
  </Tooltip>
)

export const IconsContainer = styled.div(({ theme }) => ({
  display: 'flex',
  width: '100%',
  justifyContent: 'space-between',
  paddingTop: theme.spacing.s8,
}))

export const ReadMore = styled.div(({ theme }) => ({
  color: theme.color.intent.primary.fg.default,
  cursor: 'pointer',
}))
