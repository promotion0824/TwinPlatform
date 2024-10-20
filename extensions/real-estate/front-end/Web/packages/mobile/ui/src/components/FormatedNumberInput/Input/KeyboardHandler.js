import { useWindowEventListener } from 'hooks'
import { useDropdown } from 'components/Dropdown/Dropdown'

export default function KeyboardHandler() {
  const dropdown = useDropdown()

  function getCurrentIndex(selectableNodes) {
    let activeElementIndex = [...selectableNodes].indexOf(
      window.document.activeElement
    )
    if (activeElementIndex === -1) {
      activeElementIndex = [...selectableNodes].indexOf(
        dropdown.contentRef.current?.querySelector('[data-is-selected=true]')
      )
    }
    if (activeElementIndex === -1) {
      activeElementIndex = undefined
    }

    return activeElementIndex
  }

  useWindowEventListener('keydown', (e) => {
    const selectableNodes =
      dropdown.contentRef.current.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      ) ?? []

    function up(amount) {
      e.preventDefault()

      const nextIndex =
        getCurrentIndex(selectableNodes) ?? selectableNodes.length
      selectableNodes[Math.max(nextIndex - amount, 0)]?.focus()
    }

    function down(amount) {
      e.preventDefault()

      const nextIndex = getCurrentIndex(selectableNodes) ?? -1
      selectableNodes[
        Math.min(nextIndex + amount, selectableNodes.length - 1)
      ]?.focus()
    }

    if (dropdown.contentRef.current != null) {
      if (e.key === 'ArrowUp') {
        up(1)
      } else if (e.key === 'ArrowDown') {
        down(1)
      } else if (e.key === 'PageUp') {
        up(10)
      } else if (e.key === 'PageDown') {
        down(10)
      }
    }
  })

  return null
}
