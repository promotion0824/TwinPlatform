import { Box, BoxProps } from '@mantine/core'
import { forwardRef, useState } from 'react'
import styled, { css } from 'styled-components'
import { IconButton } from '../../buttons/Button'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface SidePanelProps
  extends WillowStyleProps,
    Omit<BoxProps, keyof WillowStyleProps> {
  /** Content to be displayed in the SidePanel's header. */
  title?: React.ReactNode

  /** The main content of SidePanel */
  children?: React.ReactNode
}

/**
 * `SidePanel` is a unique layout component used primarily to create a
 * template with dedicated vertical navigation using the NavList component.
 */
export const SidePanel = forwardRef<HTMLDivElement, SidePanelProps>(
  ({ title, children, ...restProps }, ref) => {
    const [open, setOpen] = useState(true)

    const handlePanelClose = () => {
      setOpen(false)
    }

    const handlePanelOpen = () => {
      setOpen(true)
    }

    return (
      <Container
        {...restProps}
        {...useWillowStyleProps(restProps)}
        $open={open}
        ref={ref}
      >
        <HeaderSectionContainer>
          <Header $display={open}>{title}</Header>
          <IconButton
            kind="secondary"
            background="transparent"
            size="medium"
            {...(open
              ? {
                  icon: 'first_page',
                  onClick: handlePanelClose,
                  'aria-label': 'collapse',
                }
              : {
                  icon: 'last_page',
                  onClick: handlePanelOpen,
                  'aria-label': 'expand',
                })}
          />
        </HeaderSectionContainer>
        <ContentContainer $display={open}>{children}</ContentContainer>
      </Container>
    )
  }
)

const Container = styled(Box<'div'>)<{
  $open: boolean
}>(
  ({ theme, $open }) => css`
    height: 100%;
    ${$open && 'width: 320px;'}
    && {
      /* override the customized width when collapses */
      ${!$open && 'width: fit-content;'}
    }

    display: flex;
    flex-direction: column;
    align-items: center;

    background-color: ${theme.color.neutral.bg.panel.default};
    border-right: 1px solid ${theme.color.neutral.border.default};
  `
)

const HeaderSectionContainer = styled.div(
  ({ theme }) => css`
    width: 100%;
    height: fit-content;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    gap: ${theme.spacing.s8};
    padding: ${theme.spacing.s16};
  `
)

const Header = styled.div<{ $display: boolean }>(
  ({ theme, $display }) => css`
    display: ${$display ? 'initial' : 'none'};
    height: fit-content;
    color: ${theme.color.neutral.fg.default};
    ${theme.font.heading.xl2}
    flex: 1;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
  `
)
const ContentContainer = styled.div<{ $display: boolean }>(
  ({ $display }) => css`
    width: 100%;
    display: ${$display ? 'initial' : 'none'};
    overflow-y: auto;
  `
)
