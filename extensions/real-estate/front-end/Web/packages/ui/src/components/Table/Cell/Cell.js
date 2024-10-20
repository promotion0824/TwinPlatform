import cx from 'classnames'
import Text from 'components/Text/Text'
import { useHead } from '../HeadContext'
import HeadCell from './HeadCell'
import styles from '../Table.css'

export default function Cell({ type, align, className, children, ...rest }) {
  const head = useHead()

  if (head != null) {
    return (
      <HeadCell
        {...rest}
        headType={head.type}
        type={type}
        align={align}
        className={className}
      >
        {children}
      </HeadCell>
    )
  }

  const nextType = type == null ? 'td' : type

  const cxClassName = cx(
    styles.td,
    {
      [styles.typeFill]: nextType === 'fill',
      [styles.typeNone]: nextType === 'none',
      [styles.alignLeft]: align?.includes('left'),
      [styles.alignCenter]: align?.includes('center'),
      [styles.alignRight]: align?.includes('right'),
      [styles.alignTop]: align?.includes('top'),
      [styles.alignMiddle]: align?.includes('middle'),
      [styles.alignBottom]: align?.includes('bottom'),
    },
    className
  )

  return (
    <div {...rest} className={cxClassName}>
      {nextType === 'td' && <Text>{children}</Text>}
      {nextType !== 'td' && children}
    </div>
  )
}
