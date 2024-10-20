import { rem } from '@mantine/core'
import { forwardRef, HTMLProps } from 'react'
import styled from 'styled-components'

export interface PanelFooterProps extends HTMLProps<HTMLDivElement> {}

export const PanelFooter = forwardRef<HTMLDivElement, PanelFooterProps>(
  ({ children }, ref) => {
    return <PanelFooterContainer ref={ref}>{children}</PanelFooterContainer>
  }
)

const PanelFooterContainer = styled.div(({ theme }) => ({
  '&&': {
    boxSizing: 'content-box',
  },

  alignItems: 'center',
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderTop: `1px solid ${theme.color.neutral.border.default}`,
  display: 'flex',
  minHeight: rem(28),
  padding: `${theme.spacing.s8} ${theme.spacing.s16}`,
}))
