import { useEffect, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useTranslation } from 'react-i18next'
import { useEffectOnceMounted } from '@willow/common'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Timeout from 'components/Timeout/Timeout'
import OnClickOutside from 'components/OnClickOutside/OnClickOutside'
import Portal from 'components/Portal/Portal'
import Text from 'components/Text/Text'
import Icon from 'components/Icon/Icon'
import { styled } from 'twin.macro'
import { useModal, ModalContext } from './ModalContext'
import FocusTrap from './FocusTrap/FocusTrap'
import styles from './Modal.css'

export { useModal } from './ModalContext'
export { default as ModalHeader } from './ModalHeader'
export { default as ModalSubmitButton } from './ModalSubmitButton'
export { default as ModalActionButtons } from './ModalActionButtons'

export default function Modal({
  type = 'right',
  size = 'medium',
  header = undefined,

  /**
   * If true, shows a close button to the left of the content
   */
  hasCloseButton = true,

  /**
   * If true, clicking outside the modal will close it
   */
  closeOnClickOutside = true,
  children = undefined,
  className = undefined,
  contentClassName = undefined,
  onClose = undefined,
  showNavigationButtons = undefined,
  onPreviousItem = () => {},
  onNextItem = () => {},
  isFormHeader = undefined,
  icon = undefined,
  iconColor = undefined,
  isNoOverflow = undefined,
  // Scope Selector from Platform UI uses Popover with index of 300,
  // we then need this className to bump up Modal's index so that
  // when Scope Selector is open, clicking on Main Menu hamburger
  // button will open the Main Menu and it is displayed on top of
  // Scope Selector
  modalClassName = undefined,
}) {
  const modal = useModal()

  const focusTrapRef = useRef()
  const modalHeaderRef = useRef()
  const modalSubmitButtonRef = useRef()
  const { t } = useTranslation()

  const [status, setStatus] = useState('closed')
  const [response, setResponse] = useState()

  const cxClassName = cx(
    styles.modal,
    {
      [styles.hasCloseButton]: hasCloseButton,
      [styles.typeLeft]: type === 'left',
      [styles.typeRight]: type === 'right',
      [styles.typeCenter]: type === 'center',
      [styles.sizeTiny]: size === 'tiny',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.sizeLarge]: size === 'large',
      [styles.sizeExtraLarge]: size === 'extraLarge',
      [styles.sizeFull]: size === 'full',
      [styles.closing]: status === 'closing',
      [styles.closed]: status === 'closed',
      [styles['main-menu']]: modalClassName === 'main-menu',
    },
    className
  )
  const cxMaskClassName = cx(styles.mask, {
    [styles.maskTypeCenter]: type === 'center',
    [styles.closing]: status === 'closing',
    [styles.closed]: status === 'closed',
    [styles['main-menu']]: modalClassName === 'main-menu',
  })
  const cxContentClassName = cx(styles.content, {
    [styles.noOverflow]: isNoOverflow,
    contentClassName,
  })

  function close(nextResponse) {
    setResponse(nextResponse)
    setStatus('closing')
  }

  useEffect(() => {
    setStatus('opening')
  }, [])

  useEffectOnceMounted(() => {
    if (status === 'closed') {
      onClose(response)
    }
  }, [status])

  const context = {
    modalHeaderRef,
    modalSubmitButtonRef,

    close,

    closeAll() {
      context.close()

      modal?.closeAll()
    },
  }

  return (
    <ModalContext.Provider value={context}>
      <Portal>
        <div className={cxMaskClassName} />
        <OnClickOutside
          targetRefs={[focusTrapRef]}
          isClosable={closeOnClickOutside}
          onClose={() => close()}
        >
          {({ isTop }) => (
            <FocusTrap
              ref={focusTrapRef}
              isTop={isTop}
              className={cxClassName}
              onTransitionEnd={() => {
                if (status === 'opening') {
                  setStatus('opened')
                } else if (status === 'closing') {
                  setStatus('closed')
                }
              }}
            >
              <div ref={modalHeaderRef} />
              <Flex
                horizontal
                fill="header"
                align="middle"
                className={isFormHeader ? styles.formHeader : styles.header}
              >
                {_.isFunction(header) ? (
                  header(context)
                ) : (
                  <Flex
                    padding={
                      isFormHeader
                        ? 'extraLarge large'
                        : 'extraLarge 0 extraLarge extraLarge'
                    }
                    horizontal={icon ? true : undefined}
                  >
                    {icon ? (
                      <IconHeader
                        icon={icon}
                        iconColor={iconColor}
                        header={header}
                      />
                    ) : (
                      <Text type="h1">{header}</Text>
                    )}
                  </Flex>
                )}
                <div
                  ref={modalSubmitButtonRef}
                  className={styles.modalSubmitButtonContainer}
                />
                <Flex horizontal className={styles.buttonsWrapper}>
                  <Button
                    icon="close"
                    height="extraLarge"
                    tabIndex={-1}
                    className={styles.close}
                    onClick={() => close()}
                    data-segment="Close"
                    role="button"
                    aria-label="modalCloseButton"
                  />
                  {showNavigationButtons && (
                    <Flex horizontal>
                      <Button
                        isIconButton
                        height="extraLarge"
                        tabIndex={-1}
                        className={cx(styles.close, styles.prevButton)}
                        onClick={onPreviousItem}
                        data-segment="Previous modal item"
                      >
                        {t('plainText.previous')}
                      </Button>
                      <Button
                        isIconButton
                        height="extraLarge"
                        tabIndex={-1}
                        className={cx(styles.close, styles.nextButton)}
                        onClick={onNextItem}
                        data-segment="Next modal item"
                      >
                        {t('plainText.next')}
                      </Button>
                    </Flex>
                  )}
                </Flex>
              </Flex>
              <div className={cxContentClassName}>
                {_.isFunction(children) ? children(context) : children}
              </div>
            </FocusTrap>
          )}
        </OnClickOutside>
        {status === 'closing' && (
          <Timeout ms={500} onTimeout={() => onClose()} />
        )}
      </Portal>
    </ModalContext.Provider>
  )
}

const StyledText = styled(Text)`
  margin-left: 16px;
  margin-top: 4px;
  word-wrap: break-word;
  white-space: unset !important;
  flex-shrink: unset !important;
`
const StyledIcon = styled(Icon)`
  margin-top: 4px;
  margin-left: -4px;
`
function IconHeader({ icon, iconColor, header }) {
  return (
    <>
      <StyledIcon icon={icon} color={iconColor} size="xxl" />
      <StyledText type="h1" size="medium">
        {header}
      </StyledText>
    </>
  )
}
