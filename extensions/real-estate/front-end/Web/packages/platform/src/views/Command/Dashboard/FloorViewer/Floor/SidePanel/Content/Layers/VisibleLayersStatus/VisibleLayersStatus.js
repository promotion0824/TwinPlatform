import cx from 'classnames'
import { Flex } from '@willow/ui'
import styles from './VisibleLayersStatus.css'

export default function VisibleLayersStatus({
  className,
  number = 0,
  ...rest
}) {
  const cxClassName = cx(styles.wrapper, className)

  function renderDots() {
    const dots = []
    for (let i = 0; i < Math.min(number, 4); i++) {
      dots.push(<span key={i} className={styles.dot} />)
    }

    return dots
  }

  return (
    <Flex align="middle" className={cxClassName} horizontal {...rest}>
      {renderDots()}
      {number > 4 && `+${number - 4}`}
    </Flex>
  )
}
