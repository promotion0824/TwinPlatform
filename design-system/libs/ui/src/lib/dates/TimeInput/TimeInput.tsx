import {
  TimeInput as MantineTimeInput,
  TimeInputProps as MantineTimeInputProps,
} from '@mantine/dates'
import { getElementNormalizingStyle } from '@willowinc/theme'
import { ChangeEvent, forwardRef, useMemo, useState } from 'react'
import styled, { css } from 'styled-components'
import { Popover, PopoverProps } from '../../overlays/Popover'
import { BaseProps as TextInputBaseProps } from '../../inputs/TextInput/TextInput'
import { getCommonInputProps } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { TimeFormat, generateTimeItems, getValueControls } from './utils'

export interface TimeInputProps
  extends WillowStyleProps,
    Omit<
      MantineTimeInputProps,
      | keyof WillowStyleProps
      | 'withSeconds'
      | 'prefix'
      | 'value'
      | 'defaultValue'
      | 'onChange'
      | 'placeholder'
    >,
    BaseProps {}

type TimeItem = { label: string; value: string; disabled?: boolean }

interface BaseProps
  extends Omit<
    TextInputBaseProps,
    'placeholder' | 'value' | 'onChange' | 'defaultValue' | 'clearable'
  > {
  /** Default time string */
  defaultValue?: string
  /** Time string for controlled TimeInput */
  value?: string
  onChange?: (value: string) => void

  /**
   * The format of the time string shown in selection list.
   * Any format that is valid for dayjs with a combination of
   * 'H' 'HH' 'h' 'hh'
   * 'm' 'mm'
   * 's' 'ss'
   * 'a' 'A'
   * @default 'hh:mm a'
   * @example 'hh:mm:ss a' 'HH:mm' 'HH:mm:ss'
   */
  format?: TimeFormat
  /**
   * The interval for timeItems in milliseconds.
   * @default 15 * 60 * 1000
   */
  interval?: number
  /**
   * Time item lists for selection shows in popover, will use default list
   * if not provided.
   * Or you can provide a function to customize or filter the default list.
   */
  getTimes?: (defaultTimeItems: TimeItem[]) => TimeItem[]

  /** Props added to Popover component */
  popoverProps?: Partial<Omit<PopoverProps, 'children'>>
  /**
   * Maximum dropdown height
   * @default 300
   */
  maxDropdownHeight?: number
}
/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 * `TimeInput` is an input element that allows users to either select
 * a time from a predefined list or manually enter their desired time.
 */
export const TimeInput = forwardRef<HTMLInputElement, TimeInputProps>(
  (
    {
      defaultValue,
      value: controlledValue,
      onChange: setControlledValue,
      readOnly,
      getTimes,
      format = 'hh:mm a',
      interval = 15 * 60 * 1000 /* 15 minutes */,
      prefix,
      suffix,
      suffixProps,
      popoverProps,
      maxDropdownHeight = 210,
      labelWidth,
      ...restProps
    },
    ref
  ) => {
    const [opened, setOpened] = useState(false)
    const [internallyControlledValue, setInternallyControlledValue] =
      useState<string>('')
    const onChange = setControlledValue ?? setInternallyControlledValue

    const memoizedDefaultTimeList = useMemo(() => {
      return generateTimeItems({
        interval,
        format,
      })
    }, [interval, format])
    const memoizedTimeItemsList: TimeItem[] = useMemo(() => {
      if (!getTimes) {
        return memoizedDefaultTimeList
      }
      return getTimes(memoizedDefaultTimeList)
    }, [memoizedDefaultTimeList, getTimes])

    const memoizedTimeItems = useMemo(
      () =>
        memoizedTimeItemsList.map(({ label, value, disabled }) => (
          <TimeItem
            key={label}
            onClick={() => {
              setOpened(false)
              onChange(value)
            }}
            disabled={disabled}
          >
            {label}
          </TimeItem>
        )),
      [memoizedTimeItemsList, setOpened, onChange]
    )

    const handleOnChange = (e: ChangeEvent<HTMLInputElement>) => {
      onChange(e.target.value)
    }
    const handleOnOpen = () => {
      !readOnly && setOpened(true)
    }

    return (
      <Popover
        width="target"
        position="bottom-start"
        opened={opened}
        onChange={setOpened}
        {...popoverProps}
      >
        <Popover.Target>
          <MantineTimeInput
            onClick={handleOnOpen}
            onFocus={handleOnOpen} /* open dropdown on focus */
            ref={ref}
            {...restProps}
            {...useWillowStyleProps(restProps)}
            {...getCommonInputProps({ ...restProps, labelWidth })}
            withSeconds={format.includes('s')}
            leftSection={prefix}
            rightSection={suffix}
            rightSectionProps={suffixProps}
            size="xs" /* this impacts prefix and suffix space */
            {...getValueControls({
              externalValue: controlledValue,
              defaultValue,
              internalValue: internallyControlledValue,
            })}
            onChange={handleOnChange}
            readOnly={readOnly}
          />
        </Popover.Target>
        <Popover.Dropdown
          css={{ maxHeight: maxDropdownHeight, overflow: 'auto' }}
        >
          <TimeItemsContainer>{memoizedTimeItems}</TimeItemsContainer>
        </Popover.Dropdown>
      </Popover>
    )
  }
)

const TimeItem = styled.button(
  ({ theme }) => css`
    ${getElementNormalizingStyle('button')};
    width: 100%;
    border-radius: ${theme.radius.r2};
    background-color: ${theme.color.neutral.bg.panel.default};
    ${theme.font.body.md.regular};
    color: ${theme.color.neutral.fg.default};
    padding: ${theme.spacing.s4} ${theme.spacing.s8};
    display: flex;
    flex-direction: column;
    justify-content: start;

    &:hover {
      background-color: ${theme.color.intent.secondary.bg.subtle.default};
    }

    &:focus-visible {
      border-color: 'red';
    }

    &:disabled {
      color: ${theme.color.state.disabled.fg};
      background-color: unset;
    }
  `
)

const TimeItemsContainer = styled.div(
  ({ theme }) => css`
    padding: ${theme.spacing.s8};
  `
)
