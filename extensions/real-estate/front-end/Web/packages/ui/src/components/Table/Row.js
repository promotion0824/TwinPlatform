import React from 'react'
import { useHistory } from 'react-router'
import cx from 'classnames'
import styles from './Table.css'

const Row = React.forwardRef(
  (
    { to, selected, color, className, style, children, onClick, ...rest },
    ref
  ) => {
    const history = useHistory()

    const isClickable = to != null || onClick != null

    const cxClassName = cx(
      styles.row,
      {
        [styles.selected]: selected,
        [styles.clickable]: isClickable,
        [styles.hasColor]: color != null,
      },
      className
    )

    const nextStyle =
      color != null
        ? {
            '--row-color': color,
            ...style,
          }
        : style

    function handleClick(e) {
      if (to != null) {
        history.push(to)
      }

      onClick?.(e)
    }

    return (
      <div // eslint-disable-line
        ref={ref}
        {...rest}
        className={cxClassName}
        style={nextStyle}
        onClick={handleClick}
      >
        {children}
      </div>
    )
  }
)

export default Row
