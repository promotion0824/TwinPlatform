import { useEffectOnceMounted } from '@willow/common'
import { useForwardedRef } from '@willow/ui'
import { useFormControl } from 'components/Form/Form'
import Portal from 'components/Portal/Portal'
import { forwardRef, useEffect, useRef, useState } from 'react'
import DropdownContent from './DropdownContent'
import { DropdownContext } from './DropdownContext'
import DropdownHeaderButton from './DropdownHeaderButton'

export { default as DropdownButton } from './DropdownButton'
export { useDropdown } from './DropdownContext'

export default forwardRef(function Dropdown(
  {
    header,
    position,
    useMinWidth = false,
    contentClassName,
    contentStyle,
    children,
    onIsOpenChange,
    zIndex,
    ...rest
  },
  forwardedRef
) {
  const formControl = useFormControl()

  const [isOpen, setIsOpen] = useState(false)

  const dropdownRef = useForwardedRef(forwardedRef)
  const contentRef = useRef()

  useEffect(() => {
    onIsOpenChange?.(isOpen)
  }, [isOpen, onIsOpenChange])

  useEffectOnceMounted(() => {
    if (isOpen) {
      dropdownRef.current.focus()
    }
  }, [isOpen])

  useEffectOnceMounted(() => {
    formControl?.setHasFocus(isOpen)
  }, [isOpen])

  const context = {
    dropdownRef,
    contentRef,

    isOpen,

    toggle() {
      setIsOpen((prevIsOpen) => !prevIsOpen)
    },

    open() {
      setIsOpen(true)
    },

    close() {
      setIsOpen(false)
    },
  }

  return (
    <DropdownContext.Provider value={context}>
      <DropdownHeaderButton {...rest}>{header}</DropdownHeaderButton>
      {isOpen && (
        <Portal>
          <DropdownContent
            position={position}
            useMinWidth={useMinWidth}
            contentClassName={contentClassName}
            contentStyle={contentStyle}
            zIndex={zIndex}
          >
            {children}
          </DropdownContent>
        </Portal>
      )}
    </DropdownContext.Provider>
  )
})
