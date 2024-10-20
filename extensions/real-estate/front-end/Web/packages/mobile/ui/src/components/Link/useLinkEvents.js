export default function useButtonEvents(link) {
  return {
    handleClick(e) {
      if (!link.isClickable) {
        e.preventDefault()
        return
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
