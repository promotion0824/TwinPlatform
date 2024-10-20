export default function useButtonEvents({
  type,
  readOnly,
  isBlocked,
  isLink,
  preventDefault,
  ripple,
  ripplesRef,
  onClick,
  onPointerDown,
  onPointerUp,
  onPointerLeave,
}) {
  function handleButtonEvent(e) {
    if (type !== 'submit' && preventDefault) {
      if (isLink && isBlocked) {
        e.preventDefault()
      } else if (!isLink && !(isBlocked || readOnly)) {
        e.preventDefault()
      }
    }

    if (ripple) {
      ripplesRef.current.clear()
    }

    if (isBlocked || readOnly) {
      return false
    }

    return true
  }

  return {
    handleClick(e) {
      if (handleButtonEvent(e)) {
        onClick(e)
      }
    },

    handlePointerDown(e) {
      if (handleButtonEvent(e)) {
        const LEFT_BUTTON = 0
        if (ripple && (e.button === LEFT_BUTTON || e.button == null)) {
          ripplesRef.current.ripple(e)
        }

        document.activeElement?.blur?.()

        onPointerDown(e)
      }
    },

    handlePointerUp(e) {
      if (handleButtonEvent(e)) {
        onPointerUp(e)
      }
    },

    handlePointerLeave(e) {
      if (handleButtonEvent(e)) {
        onPointerLeave(e)
      }
    },
  }
}
