import { capitalize, isFunction } from 'lodash'
import { ReactNode, forwardRef, useMemo, useState } from 'react'

import { TextInput } from '../../../../inputs/TextInput'
import { Icon } from '../../../../misc/Icon'
import { DateTimeInputValue, DateTimeType } from '../../types'
import {
  QuickActionOption,
  QuickActionOptionShortcut,
  generateQuickActionOptions,
} from '../../utils/generateQuickActionOptions'
import {
  getDefaultOptionsByType,
  getOptionShortcutsByType,
} from './defaultOptionShortcuts'
import { Container, Divider, Group, InputContainer, Option } from './styles'

export interface QuickActionSelectorProps<T extends DateTimeType> {
  type: T
  /**
   * The this a helper property that accept either
   * a QuickActionOptionShortcut[] that will be formatted with label
   * 'xxx ago' or 'last xxx';
   * or a filter function that takes the default QuickActionOptionShortcut[] and
   * returns the option shortcut array.
   */
  optionShortcuts?:
    | QuickActionOptionShortcut[]
    | ((
        defaultOptions: QuickActionOptionShortcut[]
      ) => QuickActionOptionShortcut[])
  /**
   * The more customizable property that accept either
   * a QuickActionOption[] that will be rendered as quick action options;
   * or a filter function that takes the default QuickActionOption[] and
   * returns the option array.
   */
  options?:
    | QuickActionOption[][]
    | ((defaultOptions: QuickActionOption[][]) => QuickActionOption[][])

  /** Placeholder text for the search box */
  placeholder?: string
  onSelect: <T extends DateTimeType>(value: DateTimeInputValue<T>) => void
}

type QuickActionSelectorComponent = <Type extends DateTimeType>(
  props: QuickActionSelectorProps<Type> & {
    ref?: React.ForwardedRef<HTMLDivElement>
  }
) => ReactNode

/**
 * `QuickActionSelector` is a sub component that is used in a DateTimeInput,
 * to be able to select a predefined Date/DateTime or DateRange/DateTimeRange
 * with a single click.
 */
export const QuickActionSelector: QuickActionSelectorComponent = forwardRef(
  (
    {
      onSelect,
      options,
      optionShortcuts,
      type,
      placeholder = 'Find quick date',
      ...restProps
    },
    ref
  ) => {
    const [searchText, setSearchText] = useState<string>('')

    const finalOptions = useMemo(
      () =>
        getOptions({
          options,
          optionShortcuts,
          type,
        }),
      [options, optionShortcuts, type]
    )

    const filteredOptions = useMemo(() => {
      if (searchText) {
        return finalOptions.map((group) =>
          group.filter((option) =>
            option.label.toLowerCase().includes(searchText.toLowerCase())
          )
        )
      }
      return finalOptions
    }, [finalOptions, searchText])

    return (
      <Container {...restProps} ref={ref}>
        <InputContainer>
          <TextInput
            placeholder={placeholder}
            prefix={<Icon icon="search" />}
            onChange={(e) => setSearchText(e.target.value)}
          />
        </InputContainer>

        {filteredOptions.map(
          (group, index) =>
            group.length > 0 && (
              <Group key={'group' + group[0].label}>
                {group.map((option) => (
                  <Option
                    key={option.label}
                    onClick={() => {
                      onSelect(option.getValue())
                    }}
                  >
                    {capitalize(option.label)}
                  </Option>
                ))}
                {index !== filteredOptions.length - 1 && <Divider />}
              </Group>
            )
        )}
      </Container>
    )
  }
)
const getOptions = <T extends DateTimeType>({
  options,
  optionShortcuts,
  type,
}: {
  options?: QuickActionSelectorProps<T>['options']
  optionShortcuts?: QuickActionSelectorProps<T>['optionShortcuts']
  type: QuickActionSelectorProps<T>['type']
}) => {
  if (options) {
    return isFunction(options)
      ? options(getDefaultOptionsByType(type))
      : options
  }

  if (optionShortcuts) {
    return isFunction(optionShortcuts)
      ? generateQuickActionOptions(
          optionShortcuts(getOptionShortcutsByType(type))
        )
      : generateQuickActionOptions(optionShortcuts)
  }

  return getDefaultOptionsByType(type)
}
