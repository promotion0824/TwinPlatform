import { useState } from 'react'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import Text from 'components/Text/Text'
import styles from './CollapsablePanelSection.css'

export default function CollapsablePanelSection({
  name,
  header,
  className,
  children,
  adornment,
  ...rest
}) {
  const [isOpen, setIsOpen] = useState(false)

  const cxClassName = cx(
    styles.panelSection,
    {
      [styles.isOpen]: isOpen,
    },
    className
  )

  const cxHiddenContent = cx({ [styles.hiddenContent]: !isOpen })

  return (
    <Flex {...rest} className={cxClassName}>
      <Button
        className={styles.header}
        onClick={() => setIsOpen((prevIsOpen) => !prevIsOpen)}
      >
        <Flex padding="0" width="100%">
          <Flex horizontal align="middle" size="medium" padding="medium">
            <Icon icon="chevronFill" size="large" className={styles.icon} />
            <Text
              type="message"
              size="medium"
              color={isOpen ? 'white' : 'text'}
              textTransform="none"
              className={styles.content}
            >
              {header}
              {adornment && adornment}
            </Text>
          </Flex>
        </Flex>
      </Button>
      <div className={cxHiddenContent}>{children}</div>
    </Flex>
  )
}
