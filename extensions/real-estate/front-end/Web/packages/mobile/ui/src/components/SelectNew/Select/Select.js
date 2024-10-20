import { Children } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import Dropdown, { DropdownContent } from 'components/DropdownNew/Dropdown'
import Fetch from 'components/Fetch/Fetch'
import Icon from 'components/Icon/Icon'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import { SelectContext } from './SelectContext'
import KeyboardHandler from './KeyboardHandler'
import ScrollToSelectedOption from './ScrollToSelectedOption'
import styles from './Select.css'

export default function Select({
  value,
  placeholder,
  header,
  unselectable = true,
  url,
  params,
  cache,
  mock,
  notFound,
  className,
  children,
  onChange,
  ...rest
}) {
  function getOptions() {
    return (
      Children.map(children, (child) => ({
        key:
          child?.props?.value === undefined
            ? child?.props?.children
            : child?.props?.value,
        value: child?.props?.children,
      }))?.filter((option) => option.key != null) ?? []
    )
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
      [styles.hasValue]: value != null && value !== '',
      [styles.hasPlaceholder]: value == null && placeholder != null,
    },
    className
  )

  const context = {
    value,

    isSelected,
    unselectable,

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
        <DropdownContent
          role="listbox"
          useMinWidth
          contentClassName={styles.content}
        >
          <Fetch
            url={url}
            params={params}
            cache={cache}
            mock={mock}
            notFound={notFound}
            loader={
              <Spacing align="center middle">
                <Icon icon="progress" />
              </Spacing>
            }
            error={
              <Spacing align="center middle">
                <Icon icon="error" />
              </Spacing>
            }
          >
            {children}
          </Fetch>
          <ScrollToSelectedOption />
          <KeyboardHandler />
        </DropdownContent>
      </Dropdown>
    </SelectContext.Provider>
  )
}
