import {
  Drawer as MantineDrawer,
  DrawerProps as MantineDrawerProps,
} from '@mantine/core'
import styled, { css } from 'styled-components'
import { IconButton, IconButtonProps } from '../../buttons/Button'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

type DrawerSizes = 'sm' | 'md' | 'lg' | 'xl' | 'fullScreen'
export interface DrawerProps
  extends WillowStyleProps,
    Omit<
      MantineDrawerProps,
      keyof WillowStyleProps | 'title' | 'closeButtonProps' | 'size'
    > {
  /** Called when drawer is closed */
  onClose: MantineDrawerProps['onClose']
  /** Determines whether drawer is opened */
  opened: MantineDrawerProps['opened']

  /** Drawer header */
  header?: MantineDrawerProps['title']
  /** Drawer content */
  children?: MantineDrawerProps['children']
  /** Drawer footer */
  footer?: React.ReactNode

  /**
   * Controls content width
   * @type 'sm' | 'md' | 'lg' | 'xl' | 'fullScreen'
   * @default 'sm'
   */
  size?: DrawerSizes | string | number
  /**
   * Side of the screen where drawer will be opened
   * @default 'right'
   */
  position?: 'left' | 'right'

  /** @default true */
  withCloseButton?: MantineDrawerProps['withCloseButton']
  /** Props added to close button */
  closeButtonProps?: IconButtonProps
}

/**
 * `Drawer` is (TODO: add component description).
 */
// Mantine Drawer has not ref supported.
export const Drawer = ({
  header,
  children,
  footer,
  withCloseButton = true,
  closeButtonProps,
  position = 'right',
  size = 'sm',
  withOverlay = true,
  overlayProps,
  ...restProps
}: DrawerProps) => {
  const hasHeader = Boolean(header) || withCloseButton

  return (
    <StyledDrawerRoot
      {...restProps}
      {...useWillowStyleProps(restProps)}
      position={position}
      size={size in sizeMap ? sizeMap[size as DrawerSizes] : size}
    >
      {withOverlay && <MantineDrawer.Overlay {...overlayProps} />}

      <MantineDrawer.Content>
        {hasHeader && (
          <MantineDrawer.Header>
            {/* So that even no header provided, the close button will be pushed
            to the right of the header section. */}
            <MantineDrawer.Title>{header}</MantineDrawer.Title>
            {withCloseButton && (
              <IconButton
                icon="close"
                background="transparent"
                kind="secondary"
                onClick={restProps.onClose}
                {...closeButtonProps}
              />
            )}
          </MantineDrawer.Header>
        )}
        <MantineDrawer.Body>{children}</MantineDrawer.Body>
        {footer && <DrawerFooter>{footer}</DrawerFooter>}
      </MantineDrawer.Content>
    </StyledDrawerRoot>
  )
}

const StyledDrawerRoot = styled(MantineDrawer.Root)(
  ({ position, theme }) => css`
    &,
    *,
    *::before,
    *::after {
      box-sizing: border-box;
    }

    .mantine-Drawer-content {
      color: ${theme.color.neutral.fg.default};
      background-color: ${theme.color.neutral.bg.panel.default};
      display: flex;
      flex-direction: column;
      overflow-y: hidden;

      ${position === 'left'
        ? `border-right: 1px solid ${theme.color.neutral.border.default};`
        : `border-left: 1px solid ${theme.color.neutral.border.default};`}
    }

    .mantine-Drawer-header {
      h2 {
        ${theme.font.heading.lg};
      }

      background-color: ${theme.color.neutral.bg.panel.default};
      padding: ${theme.spacing.s16};
      border-bottom: 1px solid ${theme.color.neutral.border.default};
    }

    .mantine-Drawer-body {
      ${theme.font.body.md.regular};
      padding: 0;
      flex-grow: 1;
      overflow-y: auto;
    }
  `
)

const DrawerFooter = styled.div(
  ({ theme }) => css`
    ${theme.font.body.md.regular};
    padding: ${theme.spacing.s16};
    border-top: 1px solid ${theme.color.neutral.border.default};
    display: flex;
    width: 100%;
  `
)

const sizeMap: Record<DrawerSizes, number | string> = {
  sm: 380,
  md: 440,
  lg: 520,
  xl: 780,
  fullScreen: '100%',
} as const
