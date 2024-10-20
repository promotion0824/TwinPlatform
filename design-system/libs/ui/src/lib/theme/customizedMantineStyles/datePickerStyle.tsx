import { css } from 'styled-components'
import { rem } from '../../utils'

export const datePickerStyle = css(
  ({ theme }) => css`
    .mantine-DatePicker-levelsGroup {
      padding: ${theme.spacing.s12};
    }

    .mantine-DatePicker-calendarHeader {
      margin: 0;
      background-color: ${theme.color.neutral.bg.panel.default};
      max-width: unset;
    }
    .mantine-DatePicker-calendarHeaderLevel,
    .mantine-DatePicker-calendarHeaderControl, /* icons */
    .mantine-DatePicker-month,
    .mantine-DatePicker-monthThead,
    .mantine-DatePicker-weekdaysRow,
    .mantine-DatePicker-weekday,
    .mantine-DatePicker-day,
    .mantine-DatePicker-monthCell, //month
    .mantine-PickerControl-pickerControl //year
    {
      ${theme.font.body.sm.regular}
      color: ${theme.color.neutral.fg.default};
    }

    .mantine-DatePicker-day {
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

    .mantine-DatePicker-calendarHeaderControl,
    .mantine-DatePicker-calendarHeaderLevel,
    .mantine-DatePicker-day,
    .mantine-DatePicker-pickerControl {
      border-radius: ${theme.radius.r4};

      &:active {
        transform: unset; /* remove Mantine's transform */
      }
    }

    .mantine-DatePicker-pickerControl, //month
    .mantine-PickerControl-pickerControl //year
    {
      &:hover {
        background-color: ${theme.color.neutral.bg.panel.hovered};
      }
    }

    .mantine-DatePicker-calendarHeaderControl, /* icons */
    .mantine-DatePicker-calendarHeaderLevel /* month year selector */ {
      height: ${rem(36)};

      &&:hover {
        background-color: ${theme.color.neutral.bg.accent.default};
      }
    }

    .mantine-DatePicker-weekday {
      color: ${theme.color.neutral.fg.muted};
      padding: ${theme.spacing.s4} ${rem(10)};
      width: ${rem(36)};
    }

    .mantine-DatePicker-monthCell,
    .mantine-DatePicker-monthsListCell,
    .mantine-DatePicker-yearsListCell {
      text-align: center;
      vertical-align: middle;

      &[data-with-spacing] {
        padding: 0;
        margin: 1px;
      }
    }
  `
)
