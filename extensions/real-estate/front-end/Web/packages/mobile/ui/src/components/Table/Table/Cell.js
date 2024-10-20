import cx from 'classnames'
import { useTable } from './TableContext'
import styles from './Table.css'

function getPadding(padding) {
  return padding
    ?.split(' ')
    .map((str) => {
      if (str === 'tiny') return 'var(--padding-tiny)'
      if (str === 'small') return 'var(--padding-small)'
      if (str === 'medium') return 'var(--padding)'
      if (str === 'large') return 'var(--padding-large)'
      if (str === 'extra-large') return 'var(--padding-extra-large)'
      if (str === 'huge') return 'var(--padding-huge)'

      return str
    })
    .join(' ')
}

export default function Cell({
  type,
  sort,
  padding,
  className,
  width,
  style,
  children,
  ...rest
}) {
  const table = useTable()

  const isClickable = sort != null

  const cxClassName = cx(
    styles.cell,
    {
      [styles.typeFill]: type === 'fill',
      [styles.isClickable]: isClickable,
    },
    className
  )

  const nextStyle = {
    padding: getPadding(padding),
    width,
    ...style,
  }

  function handleClick() {
    table.sort(sort)
  }

  return (
    // eslint-disable-next-line
    <div
      {...rest}
      className={cxClassName}
      style={nextStyle}
      onClick={isClickable ? handleClick : undefined}
    >
      {children}
    </div>
  )
}
