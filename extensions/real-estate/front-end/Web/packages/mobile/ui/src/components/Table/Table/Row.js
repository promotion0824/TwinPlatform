import React from 'react'
import cx from 'classnames'
import Link from 'components/Link/Link'
import styles from './Table.css'

const Row = React.forwardRef(
  (
    {
      to,
      href,
      selected,
      rowColor,
      className,
      children,
      style,
      onClick,
      ...rest
    },
    ref
  ) => {
    const isClickable = to != null || href != null || onClick != null

    const cxClassName = cx(
      styles.row,
      {
        [styles.selected]: selected,
        [styles.clickable]: isClickable,
        [styles.hasRowColor]: rowColor,
      },
      className
    )

    const nextStyle =
      rowColor != null ? { '--row-color': rowColor, ...style } : style

    return to != null || href != null ? (
      <Link
        {...rest}
        ref={ref}
        to={to}
        href={href}
        className={cxClassName}
        onClick={onClick}
        style={nextStyle}
      >
        {children}
      </Link>
    ) : (
      // eslint-disable-next-line
      <div
        {...rest}
        className={cxClassName}
        onClick={onClick}
        style={nextStyle}
        ref={ref}
      >
        {children}
      </div>
    )
  }
)

export default Row
