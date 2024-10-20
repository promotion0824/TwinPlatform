import { useState } from 'react'
import styled from 'styled-components'
import { Tooltip } from '../..'

const StyledButton = styled.button(({ theme }) => ({
  fontFamily:
    "ui-monospace, Menlo, Monaco, 'Cascadia Mono', 'Segoe UI Mono', 'Roboto Mono', 'Oxygen Mono', 'Ubuntu Monospace', 'Source Code Pro', 'Fira Mono', 'Droid Sans Mono', 'Courier New', monospace",
  fontSize: theme.font.body.md.regular.fontSize,
  lineHeight: theme.font.body.lg.regular.lineHeight,
  backgroundColor: theme.color.intent.secondary.bg.muted.default,
  color: theme.color.neutral.fg.highlight,
  paddingLeft: theme.spacing.s8,
  paddingRight: theme.spacing.s8,
  borderRadius: theme.radius.r4,
  border: 'none',
  '&:hover': {
    background: theme.color.intent.secondary.bg.muted.hovered,
  },
}))

/**
 * Inline Code that allows user to copy the text to clipboard.
 */
const InlineCode = ({ text }: { text: string }) => {
  const [isCopied, setIsCopied] = useState(false)

  const copy = async () => {
    await navigator.clipboard.writeText(text)
    setIsCopied(true)

    setTimeout(() => {
      setIsCopied(false)
    }, 1300)
  }

  return (
    <Tooltip label={isCopied ? 'Copied!' : 'Copy to clipboard'} position="top">
      <StyledButton onClick={copy}>{text}</StyledButton>
    </Tooltip>
  )
}

export default InlineCode
