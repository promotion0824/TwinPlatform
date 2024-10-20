import { getElementNormalizingStyle } from '@willowinc/theme'
import { css } from 'styled-components'
import { rem } from '../../utils'

export const commonInputStyles = css(
  ({ theme }) => css`
    .mantine-InputWrapper-root.horizontal {
      display: grid !important;
      grid-template-columns: var(--willow-form-input-label-width) 1fr;

      .mantine-Input-wrapper,
      [role*='group'] {
        grid-column-start: 2;
        grid-column-end: 3;
      }

      .mantine-InputWrapper-error,
      .mantine-InputWrapper-description {
        grid-column-start: 2;
        grid-column-end: 3;
      }
    }

    .mantine-Input-section:not(.mantine-NumberInput-section) {
      &[data-position='left'] {
        padding-left: ${theme.spacing.s8};
      }

      &[data-position='right'] {
        padding-right: ${theme.spacing.s8};
        width: fit-content;
        min-width: ${rem(28)};

        svg {
          /* color of svg */
          color: ${theme.color.neutral.fg.default};
        }
      }
    }

    .mantine-Input-input {
      ${getElementNormalizingStyle('input')};

      /* default padding when there is no prefix and suffix.
      Those paddings will be added in each component themselves. */
      padding: ${theme.spacing.s4} ${theme.spacing.s8};
      opacity: 1; // override Mantine's opacity
      &:not(.mantine-Textarea-input) {
        min-height: unset;
        height: ${rem(28)};
      }

      ${theme.font.body.md.regular}
      color: ${theme.color.neutral.fg.default};
      background-color: ${theme.color.neutral.bg.panel.default};
      border: 1px solid ${theme.color.neutral.border.default};
      box-shadow: ${theme.shadow.s1};
      border-radius: ${theme.radius.r2};

      &::placeholder,
      /* for InputPlaceholder */
      .mantine-InputPlaceholder-placeholder,
      .mantine-TagsInput-inputField::placeholder {
        color: ${theme.color.neutral.fg.subtle};
        padding-left: ${theme.spacing.s2};
        opacity: 1; // for firefox
      }

      &:hover {
        background-color: ${theme.color.neutral.bg.panel.hovered};
      }

      &[readonly] {
        &:not(.mantine-Select-input) {
          background-color: ${theme.color.neutral.bg.accent.default};
        }
      }
      &:focus-visible,
      &:focus-within,
      &:focus,
      &[data-focus-visible] {
        /* invalid form component will not have focus style,
         * readOnly will have focus style. */
        &:not([aria-invalid='true']) {
          border-color: ${theme.color.state.focus.border};
        }
      }

      &[aria-invalid='true'],
      &[data-error='true'] {
        border-color: ${theme.color.intent.negative.border.default};
        background-color: ${theme.color.intent.negative.bg.subtle.default};
      }

      &:disabled,
      &[aria-disabled='true'],
      &[data-disabled='true'],
      &:has(input:disabled) /* TagsInput */ {
        background-color: ${theme.color.state.disabled.bg};
        border-color: ${theme.color.state.disabled.border};
        opacity: 1;

        &,
        &::placeholder,
        /* for InputPlaceholder */
        .mantine-InputPlaceholder-placeholder,
        .mantine-TagsInput-pill {
          color: ${theme.color.state.disabled.fg};
        }
      }

      &:is(:-webkit-autofill, :autofill) {
        -webkit-text-fill-color: ${theme.color.neutral.fg.default} !important;
        -webkit-box-shadow: 0 0 0 30px
          ${theme.color.intent.primary.bg.subtle.default} inset !important;
        transition: background-color 5000s ease-in-out 0s;
        caret-color: ${theme.color.neutral.fg.default};
      }
    }
  `
)
