import {
  Button as MantineButton,
  ButtonProps as MantineButtonProps,
} from '@mantine/core'
import { getElementNormalizingStyle, useTheme } from '@willowinc/theme'
import {
  ComponentPropsWithoutRef,
  ForwardedRef,
  MouseEventHandler,
  forwardRef,
} from 'react'
import styled from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import {
  ButtonBackground,
  ButtonKind,
  ButtonSize,
  getButtonStyles,
  getLoaderColor,
  getPadding,
} from './getButtonStyle'

export interface ButtonProps
  extends WillowStyleProps,
    Omit<
      MantineButtonProps,
      | keyof WillowStyleProps
      | 'leftSection'
      | 'rightSection'
      | 'loaderProps'
      | 'size'
      | 'style'
    >,
    Omit<ComponentPropsWithoutRef<'button'>, 'color' | 'size' | 'prefix'> {
  /** If provided, it will be prefixed to the Button. */
  prefix?: MantineButtonProps['leftSection']
  /** If provided, it will be suffixed to the Button. */
  suffix?: MantineButtonProps['rightSection']
  /** Specify the content of your Button. */
  children?: MantineButtonProps['children']
  /**
   * Specify the kind of Button you want to create.
   * @default 'primary'
   */
  kind?: ButtonKind
  /**
   * Specify the size of the Button.
   * @default 'medium'
   */
  size?: ButtonSize
  /**
   * Change the background color of button.
   * @default 'solid'
   */
  background?: ButtonBackground

  /** Provide an optional function to be called when the button element is clicked */
  onClick?: MouseEventHandler<HTMLButtonElement | HTMLAnchorElement>
  disabled?: MantineButtonProps['disabled']

  /**
   * If a href value is provided, the button will be displayed as an anchor element
   * with navigation functionality.
   */
  href?: string
  /**
   * The target attribute specifies where to open the linked page.
   */
  target?: string
}

/**
 * `Button` is a control that triggers an action. It could be rendered as a
 * button or link.
 *
 * Button labels should express what action will occur when the user interacts
 * with it.
 */
export const Button = forwardRef<
  HTMLButtonElement | HTMLAnchorElement,
  ButtonProps
>(
  (
    {
      kind = 'primary',
      size = 'medium',
      background = 'solid',
      prefix,
      suffix,
      href,
      target,
      ...restProps
    },
    ref
  ) => {
    const theme = useTheme()

    const linkElementProps =
      href !== undefined ? { component: 'a', href, target } : {}

    const loaderColor = getLoaderColor({ theme, kind, background })

    return (
      <StyledButton
        {...restProps}
        {...useWillowStyleProps(restProps)}
        ref={ref}
        $size={size}
        $kind={kind}
        $background={background}
        $prefix={prefix}
        $suffix={suffix}
        leftSection={prefix}
        rightSection={suffix}
        loaderProps={{ color: loaderColor, size: 'xs' }}
        {...linkElementProps}
      />
    )
  }
)

const StyledButton = styled(MantineButton)<
  MantineButtonProps & {
    ref: ForwardedRef<HTMLButtonElement | HTMLAnchorElement>
  } & {
    $size: Exclude<ButtonProps['size'], undefined>
    $kind: Exclude<ButtonProps['kind'], undefined>
    $background: Exclude<ButtonProps['background'], undefined>
    $prefix: ButtonProps['prefix']
    $suffix: ButtonProps['suffix']
  }
>(
  ({
    theme,
    $size: size,
    $kind: kind,
    $background: background,
    $prefix: prefix,
    $suffix: suffix,
    ...restProps
  }) => {
    const { loading } = restProps
    const stylesByBackgroundProp = getButtonStyles({
      theme,
      kind,
      background,
    })

    const paddings =
      background === 'none'
        ? 0
        : getPadding({
            size,
            prefix,
            suffix,
            theme,
          })

    return {
      ...getElementNormalizingStyle('button'),

      ...paddings,
      width: 'fit-content',
      height: 'fit-content',
      minHeight: 'unset',
      minWidth: 'unset',
      overflow: 'unset',

      '&&': {
        '--_button-loading-overlay-bg': 'unset',
      },

      // Mantine will remove outline with :focus:not(:focus-visible),
      // but we wanna keep it.
      '&, &:focus:not(:focus-visible)':
        stylesByBackgroundProp.outlineStyle || {},

      borderRadius: theme.radius.r2,
      backgroundColor: stylesByBackgroundProp.defaultBackground,

      ...theme.font.body.md.regular,
      color: stylesByBackgroundProp.defaultFontColor,
      fill: stylesByBackgroundProp.defaultFontColor,
      textDecoration: 'none', // remove underline for anchor element

      [`&:focus-visible, 
      &[data-focus-visible]` /* remove mantine style */]: {
        outline: `1px solid ${theme.color.state.focus.border}`,
        outlineOffset: '-1px',
      },

      '&:not([data-loading="true"])': {
        '&:hover': {
          color: stylesByBackgroundProp.hoveredFontColor,
          fill: stylesByBackgroundProp.hoveredFontColor,
          backgroundColor: stylesByBackgroundProp.hoveredBackground,
        },

        // active has to outweigh hover, so must be after in the cascade
        '&:active': {
          color: stylesByBackgroundProp.activatedFontColor,
          fill: stylesByBackgroundProp.activatedFontColor,
          backgroundColor: stylesByBackgroundProp.activatedBackground,
          transform: 'unset',
        },

        // disabled cannot be hovered
        '&:disabled': {
          color: theme.color.state.disabled.fg,
          fill: theme.color.state.disabled.fg,
          backgroundColor: stylesByBackgroundProp.disabledBackground,
        },
      },

      '&::before': {
        backgroundColor: 'unset',
        filter: 'unset',
        // this will remove this pseudo element,
        // not sure what it is for and it's not exist in v7.1
        content: 'none',
      },

      '.mantine-Button-inner': {
        width: 'fit-content',
        height: 'fit-content',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        gap: theme.spacing.s4,
        margin: 'auto', // center the content when button width is set

        '&&:where([data-loading])': {
          transform: 'unset',
        },
      },

      '.mantine-Button-label': {
        ...(loading && { visibility: 'hidden' }),
      },

      '.mantine-Button-loader': {
        // to override Mantine styles applied by style attribute
        transform: 'translate(-50%, -40%) !important',
      },

      '.mantine-Button-section': {
        margin: 0,
        width: theme.spacing.s20,
        height: theme.spacing.s20,

        ...(loading && { visibility: 'hidden' }),
      },
    }
  }
)
