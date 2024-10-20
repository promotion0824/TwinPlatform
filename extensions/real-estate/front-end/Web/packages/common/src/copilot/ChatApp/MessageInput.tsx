import { Textarea, Icon, Loader, useTheme, Group } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import titleCase from '@willow/common/utils/titleCase'
import { useState } from 'react'
import { styled } from 'twin.macro'

export default function MessageInput({
  placeholderText,
  loadingText,
  onSendMessage,
  isLoading = false,
  onCancelApiResponse,
}: {
  placeholderText: string
  loadingText: string
  onSendMessage: ({
    content,
    isCopilot,
  }: {
    content: string
    isCopilot: false
  }) => void
  isLoading?: boolean
  onCancelApiResponse: () => void
}) {
  const [inputText, setInputText] = useState('')
  const theme = useTheme()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const handleChange = (e) => {
    setInputText(e.target.value)
  }

  const handleSendMessage = () => {
    if (inputText.trim() !== '') {
      onSendMessage({
        content: inputText,
        isCopilot: false,
      })
      setInputText('')
    }
  }

  const handleSubmit = (e) => {
    // submit on ENTER and new-line on (SHIFT+ENTER)
    if (e.keyCode === 13 && !e.shiftKey) {
      e.preventDefault()
      handleSendMessage()
      e.target.blur()
    }
  }

  return (
    <Container>
      <Group>
        <Textarea
          placeholder={isLoading ? '' : placeholderText}
          minRows={1}
          w="100%"
          mah="80px"
          css={{
            display: 'flex',
            overflow: 'auto',

            '.mantine-Input-input': {
              padding: `${theme.spacing.s6} ${theme.spacing.s32} ${0} ${
                theme.spacing.s8
              }`,
              backgroundColor: theme.color.neutral.bg.accent.default,
            },
          }}
          leftSection={
            isLoading && (
              <>
                <Loader mr="8px" ml="78px" /> {loadingText}
              </>
            )
          }
          rightSection={
            <StyledIcon
              icon={isLoading ? 'stop_circle' : 'send'}
              onClick={isLoading ? onCancelApiResponse : handleSendMessage}
              $isAlignedBottom={isLoading ? false : inputText.length > 58}
            />
          }
          value={inputText}
          onChange={handleChange}
          onKeyDown={handleSubmit}
        />
      </Group>
      <Group
        h="80%"
        justify="center"
        css={{
          visibility: inputText.length > 58 ? 'hidden' : 'visible',
          ...theme.font.body.xs.regular,
          color: theme.color.neutral.fg.subtle,
        }}
      >
        {titleCase({ text: t('plainText.aiPoweredResultsVary'), language })}
      </Group>
    </Container>
  )
}

export const Container = styled.div(({ theme }) => ({
  width: '456px',
  padding: theme.spacing.s16,
  paddingBottom: theme.spacing.s24,
  height: '110px',
  flexDirection: 'row',
  justifyContent: 'flex-start',
  alignItems: 'flex-start',
  gap: theme.spacing.s16,
  border: `1px solid ${theme.color.neutral.border.default}`,
  background: theme.color.neutral.bg.panel.default,
}))

export const StyledIcon = styled(Icon)<{ $isAlignedBottom: boolean }>(
  ({ theme, $isAlignedBottom }) => ({
    cursor: 'pointer',
    paddingBottom: 0,
    position: $isAlignedBottom ? 'absolute' : 'relative',
    bottom: $isAlignedBottom ? 0 : undefined,
  })
)
