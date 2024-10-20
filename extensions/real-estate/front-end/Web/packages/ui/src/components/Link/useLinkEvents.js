export default function useButtonEvents(link) {
  return {
    handleClick(e) {
      if (!link.isClickable || link.isMatchingUrl) {
        e.preventDefault()
      }

      link.linkRef.current.blur()
      link.onClick?.(e)
    },

    handlePointerDown(e) {
      if (!link.isClickable) {
        e.preventDefault()
        return
      }

      link.onPointerDown(e)
    },
  }
}
