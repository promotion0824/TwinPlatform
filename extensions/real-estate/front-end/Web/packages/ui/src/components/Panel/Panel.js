import styled from 'styled-components'
import { forwardRef } from 'react'
import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import styles from './Panel.css'

const Panel = forwardRef(function Panel(
  { color = 'panel', className, children, ...rest },
  forwardedRef
) {
  const cxClassName = cx(
    styles.panel,
    {
      [styles.colorPanel]: color === 'panel',
      [styles.colorBackground]: color === 'background',
    },
    className
  )

  return (
    <Flex
      ref={forwardedRef}
      position="relative"
      {...rest}
      className={cxClassName}
    >
      {children}
    </Flex>
  )
})

const PanelWithBorder = styled(Panel)(({ $borderWidth = '1px' }) => ({
  borderWidth: $borderWidth,
}))

export default PanelWithBorder
