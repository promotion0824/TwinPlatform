import {
  SegmentedControl as MantineSegmentedControl,
  SegmentedControlItem as MantineSegmentedControlItem,
  SegmentedControlProps as MantineSegmentedControlProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled, { css, useTheme } from 'styled-components'
import { Group } from '../../layout/Group'
import { Icon, IconName } from '../../misc/Icon'
import { Tooltip } from '../../overlays/Tooltip'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface SegmentedControlItem
  extends Omit<MantineSegmentedControlItem, 'label'> {
  /**
   * If `iconOnly` is set to true, this will be the only visual representation for the segment,
   * and the `label` (if provided) will be used as a tooltip or accessible label.
   */
  iconName?: IconName
  /**
   * Acts as the main content for the segment when `iconOnly` is set to false.
   * If `iconOnly` is true and `icon` is provided, this will be used as a tooltip or accessible label for the segment.
   */
  label: React.ReactNode
  /**
   * Determines if the segment should be represented by the icon alone without displaying the label directly in the UI.
   * If set to true and `icon` is provided, the segment will show the icon as the only visual representation,
   * and the `label` (if provided) will be used as a tooltip or accessible label.
   */
  iconOnly?: boolean
}

export interface BaseProps {
  /** Segments to render */
  data: string[] | SegmentedControlItem[]

  /** Default value for uncontrolled component */
  defaultValue?: MantineSegmentedControlProps['defaultValue']
  /** Current selected value */
  value?: MantineSegmentedControlProps['value']
  /** Called when value changes */
  onChange?: MantineSegmentedControlProps['onChange']

  /** Disabled input state */
  disabled?: MantineSegmentedControlProps['disabled']
  /** Determines whether the user can change value */
  readOnly?: MantineSegmentedControlProps['readOnly']

  /**
   * True if component should have 100% width of the container
   * @default false
   */
  fullWidth?: MantineSegmentedControlProps['fullWidth']
  /**
   * The orientation of the component
   * @default 'horizontal'
   */
  orientation?: 'horizontal' | 'vertical'
}

export interface SegmentedControlProps
  extends WillowStyleProps,
    Omit<MantineSegmentedControlProps, keyof WillowStyleProps | 'data'>,
    BaseProps {}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 * `SegmentedControl` is a linear set of two or more segments.
 */
export const SegmentedControl = forwardRef<
  HTMLDivElement,
  SegmentedControlProps
>(
  (
    { data, fullWidth = false, orientation = 'horizontal', ...restProps },
    ref
  ) => {
    const theme = useTheme()
    const formattedData = data.map((item) => {
      if (typeof item !== 'string' && item.iconOnly && !item.iconName) {
        throw new Error("iconOnly is set to true, but iconName isn't provided.")
      }

      if (typeof item === 'string') {
        return item
      }

      const { label, iconName, iconOnly = false } = item

      let renderedLabel = label

      if (iconName && iconOnly) {
        renderedLabel = (
          <Tooltip label={label} withinPortal>
            <StyledIcon $label={label} icon={iconName} $iconOnly />
          </Tooltip>
        )
      } else if (iconName) {
        renderedLabel = (
          <Group gap="s4" wrap="nowrap" justify="center">
            <StyledIcon $label={label} icon={iconName} $iconOnly={false} />
            {label}
          </Group>
        )
      }

      return { ...item, label: renderedLabel }
    }) as MantineSegmentedControlProps['data']

    return (
      <StyledSegmentedControl
        {...restProps}
        {...useWillowStyleProps(restProps)}
        data={formattedData}
        fullWidth={fullWidth}
        orientation={orientation}
        ref={ref}
        color={
          restProps.disabled
            ? 'none'
            : theme.color.intent.primary.bg.bold.default
        }
      />
    )
  }
)

const StyledIcon = styled(Icon)<{
  $label: SegmentedControlItem['label']
  $iconOnly: SegmentedControlItem['iconOnly']
}>(
  ({ theme, $label, $iconOnly }) => css`
    display: block;
    /* offset some padding left added from label css when has prefix */
    margin-left: -${theme.spacing.s4};

    /* offset some padding left and right added from label css when only has prefix */
    ${(!$label || $iconOnly) &&
    css`
      margin-left: -${theme.spacing.s6};
      margin-right: -${theme.spacing.s6};
    `}
  `
)

const StyledSegmentedControl = styled(MantineSegmentedControl)(
  ({ theme }) => css`
    background-color: ${theme.color.intent.secondary.bg.subtle.default};
    padding: ${theme.spacing.s2};
    gap: 1px;
    border-radius: ${theme.radius.r2};
    /* disable animation */
    transition-duration: unset;

    input:focus:focus-visible + label {
      outline: 1px solid ${theme.color.state.focus.border};
    }
    .mantine-SegmentedControl-indicator {
      border-radius: ${theme.radius.r2};
    }

    .mantine-SegmentedControl-control {
      border: none;
      &::before {
        display: none;
      }
    }

    .mantine-SegmentedControl-label {
      &,
      .mantine-SegmentedControl-innerLabel {
        ${theme.font.heading.xs};
      }
      color: ${theme.color.neutral.fg.muted};
      border-radius: ${theme.radius.r2};
      padding: ${theme.spacing.s2} ${theme.spacing.s8};

      &:hover:not([data-disabled='true']):not([data-active='true']) {
        background-color: ${theme.color.intent.secondary.bg.subtle.hovered};
      }

      &[data-active='true'] {
        color: ${theme.color.neutral.fg.highlight};
      }

      &[data-disabled='true'] {
        color: ${theme.color.state.disabled.fg};
      }
    }
  `
)
