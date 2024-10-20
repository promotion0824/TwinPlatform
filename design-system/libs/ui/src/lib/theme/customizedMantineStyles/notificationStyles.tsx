import '@mantine/notifications/styles.css'
import { css } from 'styled-components'

export const notificationStyles = css(
  ({ theme }) => css`
    .mantine-Notifications-root {
      width: 360px;
    }

    .mantine-Notification-root {
      background: ${theme.color.neutral.bg.base.default};
      border: 1px solid ${theme.color.neutral.border.default};
      border-radius: 2px;
      border-left: 0px;
      padding: 8px 16px;

      &::before {
        left: 0px;
        bottom: 0px;
        top: 0px;
        width: 4px;
        border-radius: 0px;
      }
    }

    .mantine-Notification-title {
      margin-bottom: 0px;
    }

    .mantine-Notification-loader {
      width: 18px;
      height: 18px;

      &::after {
        width: 18px;
        height: 18px;
      }
    }

    .mantine-Notification-icon {
      background-color: transparent;
      width: 16px;
      height: 16px;
      margin: 0 8px 0 0;
      color: unset;
    }

    .mantine-Notification-closeButton {
      background-color: transparent;
      position: absolute;
      top: 8px;
      right: 8px;
      max-width: 20px;
      max-height: 20px;
      min-width: 20px;
      min-height: 20px;

      > svg {
        color: ${theme.color.neutral.fg.muted};

        &:hover {
          color: ${theme.color.neutral.fg.highlight};
        }
      }
      &:hover {
        background-color: transparent;
      }
    }
  `
)
