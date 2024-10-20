/* eslint-disable complexity */
import titleCase from '@willow/common/utils/titleCase'
import { useAnalytics, useUser } from '@willow/ui'
import _ from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import Markdown from 'react-markdown'
import { styled } from 'twin.macro'
import { Message, useChat } from './ChatContext'
import ErrorMessage from './ErrorMessage'
import MessageFooter from './MessageFooter'
import { MsgContainer } from './Styles'

export default function MessageList({
  userMessage,
  messages,
}: {
  userMessage?: string
  messages?: Message[]
}) {
  return (
    <Container>
      {[...(messages ?? []), userMessage]?.map((message) => (
        <>
          {message &&
            isValidMsg(message) &&
            (isMessageObject(message) && message.isError ? (
              <ErrorMessage errorMessage={message.content} />
            ) : (
              <MessageContentDetail message={message} />
            ))}
          <div
            // scroll the last message into view
            ref={(node) => {
              node?.scrollIntoView({ behavior: 'smooth' })
            }}
          />
        </>
      ))}
    </Container>
  )
}

export const Citations = ({ citations }: { citations: string[] }) => (
  <>
    <HairLine />
    <div tw="float-left w-full flex flex-col gap-[10px]">
      {citations.map((citation, index) => (
        <div tw="flex items-baseline space-x-3">
          <Dot>{index + 1}</Dot>
          <div tw="flex-1">{citation}</div>
        </div>
      ))}
    </div>
  </>
)

const Container = styled.div(({ theme }) => ({
  height: 'calc(100% - 250px)',
  width: '456px',
  border: `1px solid ${theme.color.neutral.border.default}`,
  background: theme.color.neutral.bg.panel.default,
  overflow: 'auto',
}))

const MsgContent = styled.div(({ theme }) => ({
  width: '100%',
  flexShrink: 0,
  overflowWrap: 'break-word',
  wordBreak: 'break-word',
  color: theme.color.neutral.fg.default,
  ...theme.font.body.lg.regular,

  '> p': {
    marginBottom: theme.spacing.s8,
  },
}))

const Dot = styled.div(({ theme }) => ({
  width: '16px',
  height: '16px',
  backgroundColor: '#d9d9d9',
  borderRadius: '60%',
  display: 'inline-block',
  color: theme.color.neutral.bg.base.default,
  textAlign: 'center',
  ...theme.font.body.xs.regular,
}))

const HairLine = styled.div(({ theme }) => ({
  height: '0.75px',
  background: theme.color.intent.secondary.border.activated,
  marginBottom: theme.spacing.s20,
}))

const MessageContentDetail = ({ message }: { message: Message | string }) => {
  const [showCitation, setShowCitation] = useState(false)

  const user = useUser()
  const analytics = useAnalytics()
  const { copilotSessionId } = useChat()

  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <MsgContainer
      $isCopilot={isMessageObject(message) ? message.isCopilot : false}
      key={isMessageObject(message) ? message.id : message}
    >
      <MsgContent>
        <Markdown>
          {isMessageObject(message) ? message.content : message}
        </Markdown>
        {isMessageObject(message) && message.isCopilot && showCitation && (
          <Citations
            citations={_.compact(
              message.citations.map((citation) =>
                citation.name && citation.pages && citation.pages.length > 0
                  ? `${citation.name} - (${titleCase({
                      text: t('plainText.page'),
                      language,
                    })} ${citation.pages.join(', ')})`
                  : citation.name
                  ? citation.name
                  : undefined
              )
            )}
          />
        )}
      </MsgContent>

      {isMessageObject(message) && message.isCopilot && (
        <MessageFooter
          onSetShowCitation={() => {
            // Only track show citations.
            if (!showCitation) {
              analytics.track('Copilot - Citation Viewed', {
                sessionID: copilotSessionId,
                userEmail: user.email,
              })
            }

            setShowCitation(!showCitation)
          }}
          message={message}
          isCitations={
            isMessageObject(message) ? message.citations.length > 0 : false
          }
          messageContent={isMessageObject(message) ? message.content : message}
          showCitation={showCitation}
        />
      )}
    </MsgContainer>
  )
}

const isValidMsg = (message) =>
  isMessageObject(message)
    ? message.content.trim() !== ''
    : message.trim() !== ''

const isMessageObject = (msg: string | Message): msg is Message =>
  (msg as Message).content !== undefined
