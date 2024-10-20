import { css } from 'styled-components'

/** Applies to all components that extend from Comboxbox. */
export const comboboxStyles = css(
  ({ theme }) => css`
    .mantine-Select-dropdown,
    .mantine-TagsInput-dropdown,
    .mantine-Combobox-dropdown {
      /* not able to restyle the gap between the input and the dropdown,
      it is dynamically calculated by Mantine. */
      box-shadow: ${theme.shadow.s4};
      padding: ${theme.spacing.s4};

      .mantine-Select-item, // v6
      .mantine-Select-option,
      .mantine-TagsInput-option,
      .mantine-Combobox-option {
        ${theme.font.body.md.regular};
        color: ${theme.color.neutral.fg.default};
        background-color: unset;
        padding: ${theme.spacing.s4} ${theme.spacing.s8};
        border-radius: ${theme.radius.r2};
        opacity: 1; // override Mantine's opacity

        /* Mantine's hover equivalent */
        &:hover:not([data-combobox-selected], [data-combobox-disabled]) {
          background-color: ${theme.color.neutral.bg.panel.hovered};
        }

        &[data-selected='true'], // v6
        &[data-checked='true'],
        &[data-combobox-selected] {
          color: ${theme.color.neutral.fg.highlight};
          background-color: ${theme.color.intent.primary.bg.bold.default};
        }

        &[data-disabled='true'] , // v6
        &[data-combobox-disabled="true"] {
          color: ${theme.color.state.disabled.fg};
          background-color: unset; // remove the background if disabled
        }
      }

      & .mantine-Select-itemsWrapper {
        padding: ${theme.spacing.s4};
      }

      .mantine-Select-separator {
        .mantine-Select-separatorLabel {
          ${theme.font.heading.group}
          color: ${theme.color.neutral.fg.muted};
        }
      }

      .mantine-Combobox-header,
      .mantine-Combobox-footer,
      .mantine-Combobox-empty {
        ${theme.font.body.md.regular};
        color: ${theme.color.neutral.fg.muted};
        padding: ${theme.spacing.s4} ${theme.spacing.s12};
      }
      .mantine-Combobox-header,
      .mantine-Combobox-footer {
        border-color: ${theme.color.neutral.border.default};
        padding: ${theme.spacing.s4} ${theme.spacing.s12};
      }
    }
  `
)
