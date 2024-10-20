import '@mantine/core/styles.css'
import '@mantine/dates/styles.css'
import { createGlobalStyle, css } from 'styled-components'

import {
  comboboxStyles,
  commonInputStyles,
  dateInputStyles,
  datePickerStyle,
  notificationStyles,
  pillStyles,
} from './customizedMantineStyles'

// This import is for the CSS of material-symbols, which is essential for using the icons
// from material-symbols in our Icon component. Ensure it remains at the top level to avoid
// multiple imports in Storybook, as this could cause random style overrides.
// see https://dev.azure.com/willowdev/Unified/_workitems/edit/79352 for details.
// The relative path is a temp fix for BUG 81807
// https://dev.azure.com/willowdev/Unified/_workitems/edit/81807
// eslint-disable-next-line @nx/enforce-module-boundaries
import '../../../../../node_modules/material-symbols/sharp.css' // options are: 'rounded' | 'sharp' | 'outlined'

const MantineStyleOverride = css(
  ({ theme }) => css`
    .mantine-InputWrapper-root {
      /* general */
      font-family: ${theme.font.body.md.regular.fontFamily};
      line-height: unset;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .mantine-Input-wrapper {
      margin-bottom: 0; /* removes the margin-bottom from Mantine */
    }

    /* field label */
    .mantine-InputWrapper-label {
      ${theme.font.body.md.regular}
      color: ${theme.color.neutral.fg.muted};
    }
    /* asterisk in field label */
    .mantine-InputWrapper-required {
      color: ${theme.color.intent.negative.fg.default};
    }

    /* field description */
    .mantine-InputWrapper-description {
      ${theme.font.body.sm.regular};
      color: ${theme.color.neutral.fg.muted};
    }

    /* field error */
    .mantine-InputWrapper-error {
      color: ${theme.color.intent.negative.fg.default};
      ${theme.font.body.sm.regular};
      display: block;
    }

    .mantine-Input-section {
      /* change the default color for prefix and suffix */
      color: ${theme.color.neutral.fg.default};
    }

    /* scrollbar */
    .mantine-ScrollArea-scrollbar,
    .mantine-ScrollArea-scrollbar:hover {
      background-color: unset;

      .mantine-ScrollArea-thumb,
      .mantine-ScrollArea-thumb:hover {
        background-color: ${theme.color.neutral.fg.subtle};
      }
    }

    /* Cannot locate MantineTooltip's target classnames like other components
     with styled-components at component level, needs to be a higher level to
     apply style to these classnames.  */
    .mantine-Tooltip-tooltip {
      ${theme.font.body.md.regular};

      color: ${theme.color.neutral.fg.default};
      background-color: ${theme.color.neutral.bg.panel.default};

      border: 1px solid ${theme.color.neutral.border.default};
      border-radius: ${theme.radius.r2};
      box-shadow: ${theme.shadow.s3};
      padding: ${theme.spacing.s8};
    }
    .mantine-Tooltip-arrow {
      background-color: ${theme.color.neutral.bg.panel.default};

      border: 1px solid ${theme.color.neutral.border.default};
    }

    /* for TimeInput */
    input[type='time']::-webkit-datetime-edit-hour-field,
    input[type='time']::-webkit-datetime-edit-minute-field,
    input[type='time']::-webkit-datetime-edit-second-field,
    input[type='time']::-webkit-datetime-edit-ampm-field,
    input::-webkit-datetime-edit-fields-wrapper {
      color: ${theme.color.neutral.fg.default};

      &:focus {
        background-color: ${theme.color.intent.primary.bg.bold.default};
        color: ${theme.color.neutral.fg.highlight};
      }
    }

    input[type='time']:disabled::-webkit-datetime-edit-hour-field,
    input[type='time']:disabled::-webkit-datetime-edit-minute-field,
    input[type='time']:disabled::-webkit-datetime-edit-second-field,
    input[type='time']:disabled::-webkit-datetime-edit-ampm-field,
    input:disabled::-webkit-datetime-edit-fields-wrapper {
      background-color: ${theme.color.state.disabled.bg};
      color: ${theme.color.state.disabled.fg};
    }

    /* for popover dropdown that is shared in multiple components */
    .mantine-Popover-dropdown {
      ${theme.font.body.md.regular};
      padding: 0;
      margin: 0;

      color: ${theme.color.neutral.fg.muted};
      background-color: ${theme.color.neutral.bg.panel.default};

      border: 1px solid ${theme.color.neutral.border.default};
      border-radius: ${theme.radius.r2};
      box-shadow: ${theme.shadow.s3};

      .mantine-ScrollArea-viewport {
        &:where([data-scrollbars='xy'], [data-scrollbars='y']):where(
            [data-offset-scrollbars='xy'],
            [data-offset-scrollbars='y']
          ) {
          padding-inline-end: unset;
          padding-inline-start: unset;
        }
      }

      .mantine-Popover-arrow {
        border: 1px solid ${theme.color.neutral.border.default};
        z-index: -1; /* This is required otherwise it will cover the content in dropdown */
      }
    }

    ${commonInputStyles}
    ${comboboxStyles}
    ${dateInputStyles}
    ${datePickerStyle}
    ${notificationStyles}
    ${pillStyles}
  `
)

const GlobalStyles = createGlobalStyle`
     ${MantineStyleOverride}
  `

export default GlobalStyles
