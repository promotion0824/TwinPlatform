import cx from 'classnames'
import { HeadContext } from './HeadContext'
import styles from './Table.css'

export default function Head({
  type = 'normal',
  isVisible = true,
  className,
  children,
  ...rest
}) {
  const cxClassName = cx(
    styles.head,
    {
      [styles.headHidden]: !isVisible,
    },
    className
  )

  return (
    <HeadContext.Provider value={{ type }}>
      <div {...rest} className={cxClassName}>
        {children}
      </div>
    </HeadContext.Provider>
  )
}
