import styled, { css } from 'styled-components'

export const Container = styled.div(
  ({ theme }) => css`
    background-color: ${theme.color.neutral.bg.panel.default};
    width: fit-content;
    display: flex;
    flex-direction: column;
  `
)

export const HorizontalContainer = styled.div`
  display: grid;
  grid-template-columns: 1fr auto;
`

export const DateInputContainer = styled.div(
  ({ theme }) => css`
    width: min-content;
    height: fit-content;
    padding: ${theme.spacing.s16};

    display: flex;
    flex-direction: column;
    gap: ${theme.spacing.s16};

    .mantine-DatePicker-levelsGroup {
      padding: 0;
    }
  `
)

export const RowContainer = styled.div(
  ({ theme }) => css`
    width: 100%;
    display: flex;
    flex-direction: row;
    gap: ${theme.spacing.s8};

    > * {
      flex-grow: 1;
    }
  `
)

export const QuickActionContainer = styled.div(
  ({ theme }) => css`
    width: fit-content;
    border-left: 1px solid ${theme.color.neutral.border.default};
    overflow-y: auto;
    display: flex;
    flex-direction: column;

    > * {
      flex-basis: 0;
      flex-grow: 1;
      overflow-y: auto;
    }
  `
)

export const FooterContainer = styled.div(
  ({ theme }) => css`
    border-top: 1px solid ${theme.color.neutral.border.default};
    padding: ${theme.spacing.s8} ${theme.spacing.s16};
    display: flex;
    gap: ${theme.spacing.s8};
    justify-content: space-between;
    align-items: center;
  `
)

export const DisplayedValueContainer = styled.div(
  ({ theme }) => css`
    ${theme.font.body.md.regular};
    color: ${theme.color.neutral.fg.default};
    flex-grow: 1;
  `
)
