import {
  Modal as MantineModal,
  ModalProps as MantineModalProps,
} from '@mantine/core'
import styled, { css } from 'styled-components'
import { IconButton, IconButtonProps } from '../../buttons/Button/IconButton'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

type Sizes = 'sm' | 'md' | 'lg' | 'xl'
export interface ModalProps
  extends WillowStyleProps,
    Omit<
      MantineModalProps,
      keyof WillowStyleProps | 'size' | 'title' | 'closeButtonProps'
    > {
  opened: MantineModalProps['opened']
  onClose: MantineModalProps['onClose']

  /** Header of the Modal content */
  header?: MantineModalProps['title']
  /** Modal's body content */
  children?: MantineModalProps['children']
  /** @default true */
  withCloseButton?: MantineModalProps['withCloseButton']
  /** Props added to close button */
  closeButtonProps?: IconButtonProps

  /**
   * Size of the modal.
   *
   * @type: 'sm' | 'md' | 'lg' | 'xl' | 'auto' | 'fullScreen'
   * @default 'sm'
   */
  size?: Sizes | 'auto' | 'fullScreen' | number | string

  /** @default false */
  centered?: MantineModalProps['centered']

  /**
   * The default behavior of the Modal component handles overflow content by
   * making the entire Modal viewport scrollable. However, if you only want the
   * content within the body section of the Modal to be scrollable, you should
   * set this property to `true`.
   *
   * @default: false
   */
  scrollInBody?: boolean
}

/**
 * `Modal` is an accessible overlay dialog.
 * @see TODO: add link to storybook
 */
// No ref supported for MantineModal
export const Modal = ({
  size = 'sm',
  header,
  onClose,
  children,
  withCloseButton = true,
  closeButtonProps,
  withOverlay = true,
  overlayProps,
  scrollInBody = false,
  centered = false,
  ...restProps
}: ModalProps) => {
  const hasHeader = !!header || withCloseButton

  return (
    <StyledModalRoot
      {...restProps}
      {...useWillowStyleProps(restProps)}
      onClose={onClose}
      fullScreen={size === 'fullScreen' || undefined}
      size={size in sizeMap ? sizeMap[size as Sizes] : size}
      $scrollInBody={scrollInBody} // only used in styled-component
      centered={centered}
    >
      {withOverlay && <MantineModal.Overlay {...overlayProps} />}
      <MantineModal.Content>
        {/* copied same render logic as MantineModal */}
        {hasHeader && (
          <MantineModal.Header>
            {<MantineModal.Title>{header}</MantineModal.Title>}
            {withCloseButton && (
              <IconButton
                icon="close"
                background="transparent"
                kind="secondary"
                onClick={onClose}
                {...closeButtonProps}
              />
            )}
          </MantineModal.Header>
        )}
        <MantineModal.Body>{children}</MantineModal.Body>
      </MantineModal.Content>
    </StyledModalRoot>
  )
}

const sizeMap: Record<Sizes, number> = {
  sm: 380,
  md: 440,
  lg: 620,
  xl: 780,
} as const

const StyledModalRoot = styled(MantineModal.Root)<{
  $scrollInBody: ModalProps['scrollInBody']
}>(
  ({ theme, $scrollInBody }) => css`
    .mantine-Modal-content {
      color: ${theme.color.neutral.fg.default};
      background-color: ${theme.color.neutral.bg.panel
        .default}; // required for body when content is small than body
      border-radius: ${theme.radius.r4};
      border: 1px solid ${theme.color.neutral.border.default};
      ${$scrollInBody &&
      css`
        display: flex;
        flex-direction: column;
      `}
    }

    .mantine-Modal-inner {
      /* For bug fix: [BUG 81714]
      (https://dev.azure.com/willowdev/Unified/_workitems/edit/81714) */
      box-sizing: border-box;
    }

    .mantine-Modal-header {
      padding: ${theme.spacing.s12} ${theme.spacing.s16};
      background-color: ${theme.color.neutral.bg.panel.default};
      border-bottom: 1px solid ${theme.color.neutral.border.default};
      min-height: unset;

      h2 {
        ${theme.font.heading.lg}
      }
    }

    .mantine-Modal-body {
      background-color: ${theme.color.neutral.bg.panel.default};
      ${theme.font.body.md.regular};
      /* So that consumer can have control to the whole Modal body area */
      padding: 0;
      margin: 0;

      ${$scrollInBody &&
      css`
        overflow: auto;
        height: 100%;
        flex: 1;
        display: flex;
      `}
    }
  `
)
