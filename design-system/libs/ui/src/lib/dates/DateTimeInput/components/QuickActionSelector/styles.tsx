import styled, { css } from 'styled-components'
import { rem } from '../../../../utils'

export const Container = styled.div(
  ({ theme }) =>
    css`
      width: ${rem(184)};
      height: 100%;
      background-color: ${theme.color.neutral.bg.panel.default};
      padding: 0 ${theme.spacing.s8};
      overflow-x: auto;
    `
)

export const InputContainer = styled.div(
  ({ theme }) => css`
    padding: ${theme.spacing.s12} 0;
    background-color: ${theme.color.neutral.bg.panel.default};
    position: sticky;
    top: 0;
  `
)

export const Group = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
`

export const Divider = styled.div(
  ({ theme }) => css`
    height: 1px;
    width: ${rem(152)};
    background-color: ${theme.color.neutral.border.default};
    margin: ${theme.spacing.s4} ${theme.spacing.s8};
  `
)

export const Option = styled.div(
  ({ theme }) => css`
    ${theme.font.body.md.regular};
    color: ${theme.color.neutral.fg.default};
    width: 100%;
    padding: ${theme.spacing.s4} ${theme.spacing.s8};
    &:hover {
      cursor: pointer;
      background-color: ${theme.color.neutral.bg.panel.hovered};
    }
  `
)
