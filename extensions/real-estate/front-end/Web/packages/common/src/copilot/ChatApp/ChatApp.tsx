import { titleCase } from '@willow/common'
import { useAnalytics, useUser } from '@willow/ui'
import {
  Button,
  ButtonGroup,
  Loader,
  Modal,
  useDisclosure,
  useTheme,
} from '@willowinc/ui'
import _ from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQueryClient } from 'react-query'
import { styled } from 'twin.macro'
import { v4 as uuidv4 } from 'uuid'
import useGetChatResponse from '../../../../platform/src/hooks/Copilot/useGetChatResponse'
import { ChatResponse, Message } from './ChatContext'
import ChatHeader from './ChatHeader'
import MessageInput from './MessageInput'
import MessageList from './MessageList'

export default function ChatApp({
  copilotSessionId,
  onResetCopilotSession,
  onToggle,
  userMessage,
  onSetMessages,
  messages,
  onSetUserMessage,
}: {
  copilotSessionId?: string
  onResetCopilotSession: () => void
  onToggle: (currActive: boolean) => void
  userMessage?: string
  onSetMessages: ({
    prevMessages,
    newUserMsg,
    response,
  }: {
    prevMessages: Message[]
    newUserMsg: string
    response?: ChatResponse
  }) => void
  messages?: Message[]
  onSetUserMessage: (currUserMsg: string) => void
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const queryClient = useQueryClient()
  const user = useUser()
  const theme = useTheme()
  const analytics = useAnalytics()

  const [confirmModalOpened, { open: modalOpen, close: modalClose }] =
    useDisclosure(false)

  const { isFetching, remove } = useGetChatResponse(
    {
      userInput: userMessage ?? '',
      options: {
        runFlags: ['CitationsMode:off'],
      },
      context: {
        sessionId: copilotSessionId ?? '',
        // LLM currently expects userId to be email, will be changed in future.
        userId: user?.email ?? undefined,
      },
    },
    {
      enabled: !!userMessage && !!copilotSessionId,
      cacheTime: 0, // Even if user input is same, communicate with Copilot API instead of depending on cache.
      select: (data) => ({
        ...data,
        isCopilot: true,
        id: uuidv4(),
        content: data.responseText,
        citations:
          data.citations?.filter(
            (item) => !['context', 'history'].includes(item.name)
          ) ?? [],
        showCitation: false,
      }),
      onSuccess: (data) => {
        analytics.track('Copilot - Answer Received', {
          time: new Date().toISOString(),
          sessionID: copilotSessionId,
          userEmail: user.email,
        })

        onSetMessages({
          prevMessages: messages ?? [],
          newUserMsg: userMessage ?? '',
          response: data,
        })
        onSetUserMessage('')
      },
      onError: () => {
        onSetMessages({
          prevMessages: messages ?? [],
          newUserMsg: userMessage ?? '',
          response: {
            responseText: t('plainText.errorGeneratingResponse'),
            content: t('plainText.errorGeneratingResponse'),
            citations: [],
            isError: true,
          },
        })
      },
    }
  )

  const handleCancelApiResponse = () => {
    queryClient.cancelQueries(['chat-response'])
    onSetMessages({
      prevMessages: messages ?? [],
      newUserMsg: userMessage ?? '',
    })
  }

  const handleSendMessage = (inputMessage) => {
    if (inputMessage.content.trim() === '') {
      return
    }

    analytics.track('Copilot - Question Asked', {
      time: new Date().toISOString(),
      sessionID: copilotSessionId,
      userEmail: user.email,
    })

    onSetUserMessage(inputMessage.content)

    // Even if user input is same, communicate with Copilot API (So remove query from Cache)
    remove()
  }

  return (
    <Container>
      <ChatHeader
        headerText={t('headers.willowCopilot')}
        badgeText={_.startCase(t('headers.preview'))}
        isResetDisabled={
          messages
            ? messages.every((item) => item?.content === '' || item == null)
            : true
        }
        onReset={() => {
          modalOpen()
        }}
        onClose={() => {
          analytics.track('Copilot - Close Button Clicked', {
            sessionID: copilotSessionId,
            userEmail: user.email,
          })

          onToggle(false)
        }}
      />
      <Modal
        opened={confirmModalOpened}
        onClose={() => (confirmModalOpened ? modalClose() : onToggle(false))}
        header={titleCase({ text: t('headers.clearChat'), language })}
        size="sm"
        styles={{
          inner: {
            padding: 0,
          },
          content: {
            position: 'relative',
            marginTop: '120px',
            marginLeft: 'auto',
            marginRight: '42px',
            height: '180px',
            overflowY: 'hidden',
          },
          header: {
            overflowY: 'hidden',
          },
          body: {
            overflowY: 'auto',
            height: 'calc(100% - 48px)',
          },
          overlay: {
            marginTop: '0px',
            position: 'fixed',
            right: 0,
            height: '100%',
            width: '456px',
            textAlign: 'right',
            left: 'revert',
            marginRight: theme.spacing.s16,
            zIndex: 200,
            top: '60px',
          },
        }}
      >
        <Content>
          <>
            {t('plainText.clearChatDescription')}
            <ButtonGroup tw="self-end gap-[8px]">
              <Button kind="secondary" onClick={modalClose}>
                {t('plainText.cancel')}
              </Button>
              <Button
                onClick={() => {
                  analytics.track('Copilot - Clear Chat Clicked', {
                    userEmail: user.email,
                  })

                  onSetMessages({
                    prevMessages: [],
                    newUserMsg: '',
                  })
                  onSetUserMessage('')
                  modalClose()
                  onResetCopilotSession()
                }}
                disabled={isFetching}
              >
                {isFetching ? <Loader /> : t('plainText.clear')}
              </Button>
            </ButtonGroup>
          </>
        </Content>
      </Modal>
      <MessageList
        messages={messages}
        userMessage={isFetching ? userMessage : undefined}
      />
      <MessageInput
        placeholderText={t('plainText.msgWillowCopilot')}
        loadingText={t('plainText.searchText')}
        isLoading={isFetching}
        onCancelApiResponse={handleCancelApiResponse}
        onSendMessage={handleSendMessage}
      />
    </Container>
  )
}

const Container = styled.div(({ theme }) => ({
  marginTop: '0px',
  position: 'fixed',
  right: 0,
  marginRight: `${theme.spacing.s16}`,
  zIndex: theme.zIndex.popover,
  top: '60px',
  height: '100%',
}))

const Content = styled.div(({ theme }) => ({
  display: 'flex',
  padding: theme.spacing.s16,
  flexDirection: 'column',
  justifyContent: 'center',
  alignItems: 'center',
  gap: theme.spacing.s8,
  flex: '1 0 0',
}))
