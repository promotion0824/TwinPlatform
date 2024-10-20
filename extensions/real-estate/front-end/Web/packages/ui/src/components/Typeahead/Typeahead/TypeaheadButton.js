import { useRef } from 'react'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Text from 'components/Text/Text'
import { useTypeahead } from './TypeaheadContext'
import styles from './TypeaheadButton.css'

export default function TypeaheadButton({
  type,
  value,
  className,
  children,
  onClick,
  ...rest
}) {
  const typeahead = useTypeahead()

  const buttonRef = useRef()

  const isSelected = typeahead.selected && typeahead.isSelected(children)

  const cxClassName = cx(
    styles.typeaheadButton,
    {
      [styles.typeHeader]: type === 'header',
    },
    className
  )

  return (
    <Button
      ref={buttonRef}
      selected={isSelected}
      {...rest}
      className={cxClassName}
      onClick={(e) => {
        typeahead.select(value ?? children)

        onClick?.(e)
      }}
    >
      <Text
        type={type === 'header' ? 'message' : undefined}
        whiteSpace="nowrap"
      >
        {children}
      </Text>
    </Button>
  )
}
