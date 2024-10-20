import { Children } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import Dropdown, { DropdownContent } from 'components/Dropdown/Dropdown'
import Text from 'components/Text/Text'
import { SelectContext } from './SelectContext'
import KeyboardHandler from './KeyboardHandler'
import ScrollToSelectedOption from './ScrollToSelectedOption'
import styles from './Select.css'

export default function Select({
  value,
  placeholder,
  header,
  unselectable = false,
  className,
  children,
  onChange,
  ...rest
}) {
  function getOptions() {
    return Children.map(children, (child) => ({
      key: child?.props?.value ?? child?.props?.children,
      value: child?.props?.children,
    })).filter((option) => option.key != null)
  }

  function isSelected(nextValue) {
    return _.isEqual(nextValue, value)
  }

  const options = getOptions()

  let formattedValue = value ?? placeholder
  if (value != null) {
    formattedValue =
      header == null
        ? options.find((option) => isSelected(option.key))?.value
        : header(value, options)
  }

  const cxClassName = cx(
    {
      [styles.hasValue]: value != null,
      [styles.hasPlaceholder]: value == null && placeholder != null,
    },
    className
  )

  const context = {
    value,

    isSelected,

    select(nextValue) {
      const derivedValue =
        !unselectable || !context.isSelected(nextValue) ? nextValue : null

      onChange(derivedValue)
    },
  }

  return (
    <SelectContext.Provider value={context}>
      <Dropdown {...rest} className={cxClassName}>
        <Text className={styles.text}>{formattedValue}</Text>
        <DropdownContent contentClassName={styles.content}>
          {children}
          <ScrollToSelectedOption />
          <KeyboardHandler />
        </DropdownContent>
      </Dropdown>
    </SelectContext.Provider>
  )
}
