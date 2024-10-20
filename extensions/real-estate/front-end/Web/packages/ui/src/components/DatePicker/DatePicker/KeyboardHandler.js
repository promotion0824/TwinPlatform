import { useWindowEventListener } from '@willow/ui'
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
    const selectableNodes = Array.from(
      dropdown.contentRef.current.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      ) ?? []
    )

    function up(amount) {
      e.preventDefault()
      const nextIndex = getCurrentIndex(selectableNodes) ?? 3
      selectableNodes[Math.max(nextIndex - amount, 0)]?.focus()
    }

    function down(amount) {
      e.preventDefault()
      const nextIndex = getCurrentIndex(selectableNodes) ?? 1
      selectableNodes[
        Math.min(nextIndex + amount, selectableNodes.length - 1)
      ]?.focus()
    }

    if (dropdown.contentRef.current != null) {
      if (e.key === 'ArrowUp') {
        up(6)
      } else if (e.key === 'ArrowDown') {
        down(6)
      } else if (e.key === 'ArrowLeft') {
        up(1)
      } else if (e.key === 'ArrowRight') {
        down(1)
      } else if (e.key === 'Tab') {
        dropdown.dropdownRef.current.focus()
        dropdown.close()
      } else if (e.key !== 'Enter') {
        // Skip when target is *text input fields* to ensure event is propagated down
        // so that keystroke input are accepted by text input.
        if (e.target.nodeName === 'INPUT' && e.target.type === 'text') {
          return
        }

        e.preventDefault()
      }
    }
  })

  return null
}
