import tw from 'twin.macro'
import cx from 'classnames'
import { useUserAgent } from '@willow/ui'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Text from 'components/Text/Text'
import styles from './Card.css'

export { default as CardButton } from './CardButton'

export default function Card({
  header,
  selected,
  className,
  children,
  ...rest
}) {
  const userAgent = useUserAgent()

  const cxClassName = cx(
    styles.card,
    {
      [styles.selected]: selected,
      [styles.isIpad]: userAgent.isIpad,
    },
    className
  )

  return (
    <div className={cxClassName}>
      <Button
        {...rest}
        className={styles.button}
        data-testid={`card-${header}`}
      />
      <Flex
        tw="w-full"
        fill="header"
        padding="large"
        className={styles.content}
      >
        <Text type="h3">{header}</Text>
      </Flex>
      {children}
    </div>
  )
}
