/* eslint-disable complexity */
import { forwardRef } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { passToFunction } from '@willow/ui'
import Dropdown from 'components/Dropdown/Dropdown'
import Fetch from 'components/Fetch/Fetch'
import getOptions from './getOptions'
import { SelectContext } from './SelectContext'
import ScrollToSelectedOption from './ScrollToSelectedOption'
import styles from './Select.css'

export default forwardRef(function Select(
  {
    header,
    value,
    error,
    placeholder,
    readOnly,
    disabled,
    unselectable,
    height,
    border = 'top left bottom right',
    isPillSelect,
    url,
    params,
    cache,
    notFound,
    mock,
    className,
    children,
    onChange,
    isFontLight = false,
    partialValueCheckDisabled = false,
    formatPlaceholder = true, // When true, format placeholder to `- ${placeholder} -`
    isMultiSelect = false,
    ...rest
  },
  forwardedRef
) {
  function isSelected(nextValue) {
    let valueToCompare = nextValue

    if (
      !partialValueCheckDisabled &&
      _.isObject(value) &&
      _.isObject(nextValue)
    ) {
      valueToCompare = Object.keys(value).reduce(
        (obj, key) => ({
          ...obj,
          [key]: nextValue[key],
        }),
        {}
      )
    }

    return _.isEqual(value, valueToCompare)
  }

  const options = getOptions(children)

  const nextPlaceholder =
    placeholder != null
      ? formatPlaceholder
        ? `- ${placeholder} -`
        : placeholder
      : undefined

  let formattedValue = value ?? nextPlaceholder ?? ''

  if (header != null) {
    formattedValue = header(value, options)
    if (formattedValue === '' || formattedValue == null) {
      formattedValue = nextPlaceholder ?? ''
    }
  } else if (value != null) {
    formattedValue =
      options.find((option) => isSelected(option.key))?.value ?? value

    if (!_.isString(formattedValue)) {
      formattedValue = ''
    }
  }

  const context = {
    unselectable,
    isSelected,
    isMultiSelect,

    select(nextValue) {
      if (!unselectable && _.isEqual(value, nextValue)) {
        onChange(null)
      } else {
        onChange(nextValue)
      }
    },
  }

  return (
    <SelectContext.Provider value={context}>
      <Dropdown
        ref={forwardedRef}
        {...rest}
        header={formattedValue}
        readOnly={readOnly}
        disabled={disabled}
        useMinWidth
        className={(dropdown) =>
          cx(
            styles.select,
            {
              [styles.open]: dropdown.isOpen,
              [styles.hasValue]: value != null && value !== '',
              [styles.hasPlaceholder]: value == null && nextPlaceholder != null,
              [styles.hasError]: error != null,
              [styles.readOnly]: readOnly,
              [styles.disabled]: disabled,
              [styles.heightLarge]: height === 'large',
              [styles.borderTop]: border?.split(' ').includes('top'),
              [styles.borderLeft]: border?.split(' ').includes('left'),
              [styles.borderBottom]: border?.split(' ').includes('bottom'),
              [styles.borderRight]: border?.split(' ').includes('right'),
              [styles.pillSelect]: isPillSelect,
            },
            className
          )
        }
        headerContentClassName={`${isFontLight ? '' : styles.headerContent}`}
        contentClassName={styles.content}
        data-error={error != null ? true : undefined}
      >
        {url != null ? (
          <Fetch
            url={url}
            params={params}
            cache={cache}
            notFound={notFound}
            mock={mock}
          >
            {(...args) => (
              <>
                {passToFunction(children, ...args)}
                <ScrollToSelectedOption />
              </>
            )}
          </Fetch>
        ) : (
          <>
            {children}
            <ScrollToSelectedOption />
          </>
        )}
      </Dropdown>
    </SelectContext.Provider>
  )
})
