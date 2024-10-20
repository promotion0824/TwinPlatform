import { useEffect } from 'react'
import _ from 'lodash'
import { useOnClickOutsideIds } from 'providers'
import { useUniqueId, useWindowEventListener } from 'hooks'
import { useLatest } from '@willow/common'

function containsElement(targetRefs, element) {
  return targetRefs.some((targetRef) => targetRef.current?.contains(element))
}

function shouldIgnoreElement(element) {
  if (element == null) {
    return false
  }

  if (element.classList?.contains('ignore-onclickoutside')) {
    return true
  }

  return shouldIgnoreElement(element.parentNode)
}

export default function OnClickOutside({
  targetRefs,
  isClosable = true,
  closeOnWheel = false,
  children,
  onClose,
}) {
  const onClickOutsideIds = useOnClickOutsideIds()
  const onClickOutsideId = useUniqueId()

  const latestOnClose = useLatest(onClose)

  const isTop = onClickOutsideIds.isTop(onClickOutsideId)

  useEffect(() => {
    onClickOutsideIds.registerOnClickOutsideId(onClickOutsideId)

    return () => onClickOutsideIds.unregisterOnClickOutsideId(onClickOutsideId)
  }, [])

  function handlePointerDown(e) {
    if (isTop && isClosable) {
      const LEFT_BUTTON = 0
      if (e.button === LEFT_BUTTON || e.button == null) {
        if (
          !containsElement(targetRefs, e.target) &&
          !shouldIgnoreElement(e.target)
        ) {
          latestOnClose()
        }
      }
    }
  }

  function handleKeydown(e) {
    if (isTop && isClosable) {
      if (e.key === 'Escape') {
        latestOnClose()
      }
    }
  }

  function handleWheel(e) {
    if (isTop && isClosable && closeOnWheel) {
      if (!containsElement(targetRefs.slice(-1), e.target)) {
        latestOnClose()
      }
    }
  }

  useWindowEventListener('pointerdown', handlePointerDown, true)
  useWindowEventListener('keydown', handleKeydown)
  useWindowEventListener('wheel', handleWheel)

  return _.isFunction(children) ? children({ isTop }) : children ?? null
}
