import { css } from 'styled-components'
import { rem } from '../../utils'

// shared for Pill and TagInput
export const pillStyles = css(
  ({ theme }) => css`
    .mantine-PillGroup-group {
      gap: ${theme.spacing.s4};
      flex-wrap: wrap;
    }

    .mantine-Pill-root {
      height: ${theme.spacing.s20};
      padding: ${theme.spacing.s2} ${theme.spacing.s8};
      background-color: ${theme.color.neutral.bg.base.default};
      border: 1px solid ${theme.color.neutral.border.default};
      border-radius: ${theme.radius.pill};
      display: flex;
      align-items: center;
      justify-content: center;
      gap: ${theme.spacing.s2};
      width: fit-content;

      color: ${theme.color.neutral.fg.highlight};
      &[data-disabled] {
        color: ${theme.color.state.disabled.fg};
      }

      .mantine-Pill-label {
        height: fit-content;
        ${theme.font.body.xs.regular};
      }
      .mantine-Pill-remove {
        padding: 0;
        height: ${rem(16)};
        min-width: ${rem(16)};
      }
      > button {
        margin-right: -${theme.spacing.s4};
      }
    }
  `
)
