import styled from 'styled-components'
import { useState } from 'react'
import cx from 'classnames'
import Button from '../Button/Button'
import Flex from '../Flex/Flex'
import Tabs, { Tab } from '../Tabs/Tabs'
import styles from './CollapsablePanel.css'

// TODO: Convert CollapsablePanel component to TSX format
// https://dev.azure.com/willowdev/Unified/_workitems/edit/76840

export default function CollapsablePanel({
  header,
  position = 'right',
  icon = 'chevronBack',
  children,
  onPanelStateChange = () => {},
  border = undefined,
  $borderWidth = '1px 1px 0px 0px',
  width = undefined,
  noScroll = undefined,
  isOpen: isOpenProp = undefined, // Use as controlled component if "isOpen" is defined
  className = undefined,
  ...rest
}) {
  const isControlledComponent = isOpenProp !== undefined
  const [isOpenState, setIsOpenState] = useState(true)
  const isOpen = isControlledComponent ? isOpenProp : isOpenState

  const cxClassName = cx(
    styles.collapsablePanel,
    {
      [styles.open]: isOpen,
      [styles.closed]: !isOpen,
      [styles.positionLeft]: position === 'left',
      [styles.positionRight]: position === 'right',
      [styles.noBorder]: border === 'none',
      [styles.widthSmall]: width === 'small',
      [styles.noScroll]: noScroll,
    },
    className
  )

  const cxToggle = cx(styles.toggle, {
    [styles.toggleClosed]: !isOpen,
  })

  function handleClick() {
    onPanelStateChange(!isOpen)
    setIsOpenState((prevIsOpen) => !prevIsOpen)
  }

  return (
    <StyledContainer
      {...rest}
      className={cxClassName}
      onTransitionEnd={(e) => {
        if (e.propertyName === 'max-width') {
          window.dispatchEvent(new Event('resize'))
        }
      }}
    >
      <Tabs className={styles.tabs} $borderWidth={$borderWidth}>
        <Tab header={header}>{children}</Tab>
      </Tabs>
      <Button icon={icon} className={cxToggle} onClick={handleClick} />
    </StyledContainer>
  )
}

// reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/76029
const StyledContainer = styled(Flex)({
  '& [role="tablist"]': {
    alignItems: 'center',
    // to ensure first tab will have border bottom
    '&::before': {
      zIndex: 1,
    },
  },
  '& [role="tab"]': {
    borderRight: 'none',

    '& > span': {
      font: '500 13px/20px Poppins',
    },
  },
})
