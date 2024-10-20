import {
  Tabs as MantineTabs,
  TabsPanelProps as MantineTabsPanelProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled, { css } from 'styled-components'

export interface PanelProps extends MantineTabsPanelProps {
  value: MantineTabsPanelProps['value']
  children: MantineTabsPanelProps['children']
}

/**
 * `Tabs.Panel` is the element that contains the content associated with a tab.
 *
 * @see TODO: add link to storybook
 */
export const Panel = forwardRef<HTMLDivElement, PanelProps>(
  ({ ...restProps }, ref) => {
    return <StyledPanel {...restProps} ref={ref} />
  }
)

const StyledPanel = styled(MantineTabs.Panel)(
  ({ theme }) => css`
    ${theme.font.body.md.regular};
    color: ${theme.color.neutral.fg.default};
    padding: 0;
    margin: 0;
  `
)
