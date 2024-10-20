import { useWindowEventListener } from '@willow/ui'
import { useTypeahead } from './TypeaheadContext'

export default function KeyboardHandler() {
  const typeahead = useTypeahead()

  useWindowEventListener('keydown', (e) => {
    const selectableNodes = [
      ...(typeahead.contentRef.current?.querySelectorAll(
        'button:not(:disabled):not([tabindex="-1"]), [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      ) ?? []),
    ]

    function up(amount) {
      const activeElement = typeahead.contentRef.current?.contains(
        document.activeElement
      )
        ? document.activeElement
        : typeahead.contentRef.current?.querySelector(
            '[data-is-selected=true]'
          ) ?? document.activeElement

      const index = selectableNodes.indexOf(activeElement)

      if (index === 0) {
        typeahead.inputRef.current.focus()
      } else if (index > 0) {
        const nextIndex = Math.max(index - amount, 0)

        selectableNodes[nextIndex]?.focus()
      }
    }

    function down(amount) {
      const activeElement = typeahead.contentRef.current?.contains(
        document.activeElement
      )
        ? document.activeElement
        : typeahead.contentRef.current?.querySelector(
            '[data-is-selected=true]'
          ) ?? document.activeElement

      const index = selectableNodes.indexOf(activeElement)

      if (index >= 0) {
        const nextIndex = Math.min(index + amount, selectableNodes.length - 1)

        selectableNodes[nextIndex]?.focus()
      } else {
        selectableNodes[0]?.focus()
      }
    }

    if (e.key === 'ArrowUp') {
      e.preventDefault()

      up(1)
    }

    if (e.key === 'ArrowDown') {
      e.preventDefault()

      down(1)
    }

    if (e.key === 'PageUp') {
      e.preventDefault()

      up(10)
    }

    if (e.key === 'PageDown') {
      e.preventDefault()

      down(10)
    }

    if (e.key === 'Tab') {
      typeahead.inputRef.current.focus()
      typeahead.close()
      typeahead.onBlur()
    }

    if (e.key === 'Escape') {
      typeahead.inputRef.current.focus()
    }
  })

  return null
}
