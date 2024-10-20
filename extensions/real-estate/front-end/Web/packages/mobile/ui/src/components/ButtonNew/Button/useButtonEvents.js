export default function useButtonEvents(button) {
  function handleButtonEvent(e) {
    if (button.preventDefault && (!button.isLink || button.isDisabled)) {
      e.preventDefault()
    }

    if (button.ripple) {
      button.ripplesRef.current.clear()
    }

    if (button.readOnly) {
      return false
    }

    return !button.isDisabled
  }

  return {
    handleClick(e) {
      if (handleButtonEvent(e)) {
        button.onClick(e)
      }
    },

    handlePointerDown(e) {
      if (handleButtonEvent(e)) {
        const LEFT_BUTTON = 0
        if (button.ripple && (e.button === LEFT_BUTTON || e.button == null)) {
          button.ripplesRef.current.ripple(e)
        }

        button.onPointerDown(e)
      }
    },

    handlePointerUp(e) {
      if (handleButtonEvent(e)) {
        button.onPointerUp(e)
      }
    },

    handlePointerLeave(e) {
      if (handleButtonEvent(e)) {
        button.onPointerLeave(e)
      }
    },
  }
}
