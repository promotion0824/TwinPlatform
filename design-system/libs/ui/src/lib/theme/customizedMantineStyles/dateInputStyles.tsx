import { css } from 'styled-components'
import { rem } from '../../utils'

export const dateInputStyles = css(
  ({ theme }) => css`
    table.mantine-DateInput-month {
      border-spacing: ${rem(1)};
      border-collapse: separate;
    }

    .mantine-DateInput-levelsGroup {
      padding: ${theme.spacing.s12};
    }

    .mantine-DateInput-calendarHeader {
      margin: 0;
      background-color: ${theme.color.neutral.bg.panel.default};
      max-width: unset;
    }
    .mantine-DateInput-calendarHeaderLevel,
    .mantine-DateInput-calendarHeaderControl, /* icons */
    .mantine-DateInput-month,
    .mantine-DateInput-monthThead,
    .mantine-DateInput-weekdaysRow,
    .mantine-DateInput-weekday,
    .mantine-DateInput-day,
    .mantine-DateInput-pickerControl, //month
    .mantine-PickerControl-pickerControl //year
    {
      ${theme.font.body.sm.regular}
      color: ${theme.color.neutral.fg.default};
    }

    .mantine-DateInput-day {
      width: ${rem(36)};
      height: ${rem(36)};
      padding: ${rem(10)};
      color: ${theme.color.neutral.fg.default};
      opacity: 1; // override Mantine's opacity

      &&:hover {
        background-color: ${theme.color.neutral.bg.panel.hovered};
      }

      &[data-outside] {
        color: ${theme.color.neutral.fg.subtle};
      }

      &[data-weekend] {
        color: ${theme.color.intent.negative.fg.default};
      }

      &[data-selected] {
        color: ${theme.color.neutral.fg.highlight};
        background-color: ${theme.color.intent.primary.bg.bold.default};
        opacity: 1;
        &:hover {
          /* need this to override mantine's selected + hover style */
          background-color: ${theme.color.intent.primary.bg.bold.hovered};
        }
      }

      &:disabled {
        color: ${theme.color.state.disabled.fg};
        background-color: unset;
        &:hover {
        }
      }
    }

    .mantine-DateInput-calendarHeaderControl,
    .mantine-DateInput-calendarHeaderLevel,
    .mantine-DateInput-day,
    .mantine-DateInput-pickerControl {
      border-radius: ${theme.radius.r4};

      &:active {
        transform: unset; /* remove Mantine's transform */
      }
    }

    .mantine-DateInput-pickerControl, //month
    .mantine-PickerControl-pickerControl //year
    {
      &:hover {
        background-color: ${theme.color.neutral.bg.panel.hovered};
      }
    }

    .mantine-DateInput-calendarHeaderControl  /* icons */ {
      padding: ${rem(10)};
      width: ${rem(36)};
      height: ${rem(36)};

      &&:hover {
        background-color: ${theme.color.neutral.bg.accent.default};
      }
    }

    .mantine-DateInput-calendarHeaderLevel /* month year selector */ {
      padding: ${rem(10)};
      height: ${rem(36)};

      &&:hover {
        background-color: ${theme.color.neutral.bg.accent.default};
      }
    }

    .mantine-DateInput-weekday {
      color: ${theme.color.neutral.fg.muted};
      padding: 0;
      width: ${rem(36)};
      height: ${rem(24)};
      text-align: center;
      vertical-align: middle;
    }

    .mantine-DateInput-monthCell,
    .mantine-DateInput-monthsListCell,
    .mantine-DateInput-yearsListCell {
      text-align: center;
      vertical-align: middle;

      &[data-with-spacing] {
        padding: 0;
        margin: 0;
      }
    }
  `
)
