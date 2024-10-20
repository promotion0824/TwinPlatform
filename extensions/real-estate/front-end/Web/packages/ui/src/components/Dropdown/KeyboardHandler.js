import { useEffect, useState } from 'react'
import { useTimer, useWindowEventListener } from '@willow/ui'
import { useDropdown } from './DropdownContext'

export default function KeyboardHandler() {
  const dropdown = useDropdown()
  const timer = useTimer()

  const [search, setSearch] = useState('')

  function getActiveElementIndex(selectableElements) {
    let activeElementIndex = [...selectableElements].indexOf(
      window.document.activeElement
    )
    if (activeElementIndex === -1) {
      activeElementIndex = [...selectableElements].indexOf(
        dropdown.contentRef.current?.querySelector('[data-is-selected=true]')
      )
    }
    if (activeElementIndex === -1) {
      activeElementIndex = undefined
    }

    return activeElementIndex
  }

  useEffect(() => {
    async function update() {
      if (search !== '') {
        const items = [
          ...dropdown.contentRef.current.childNodes[0].childNodes,
        ].map((childNode) => ({
          node: childNode,
          text: childNode.innerText?.toLowerCase() ?? '',
        }))

        let selectedItem = items.find((item) => item.text.startsWith(search))
        if (selectedItem == null) {
          selectedItem = items.find((item) => item.text.includes(search))
        }

        selectedItem?.node?.focus()

        await timer.setTimeout(500)

        setSearch('')
      }
    }

    update()
  }, [search])

  function handleKeydown(e) {
    if (dropdown.contentRef.current == null) {
      return
    }

    const selectableElements =
      dropdown.contentRef.current.querySelectorAll(
        'button:not([tabindex="-1"]), [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      ) ?? []

    function up(amount) {
      e.preventDefault()

      const nextIndex =
        getActiveElementIndex(selectableElements) ?? selectableElements.length
      selectableElements[Math.max(nextIndex - amount, 0)]?.focus()
    }

    function down(amount) {
      e.preventDefault()

      const nextIndex = getActiveElementIndex(selectableElements) ?? -1

      selectableElements[
        Math.min(nextIndex + amount, selectableElements.length - 1)
      ]?.focus()
    }

    if (e.key === 'ArrowUp') {
      up(1)
    } else if (e.key === 'ArrowDown') {
      down(1)
    } else if (e.key === 'Tab') {
      dropdown.dropdownRef.current.focus()
      dropdown.close()
    } else if (e.key === 'PageUp') {
      up(10)
    } else if (e.key === 'PageDown') {
      down(10)
    } else if (e.key.length === 1) {
      // Skip when target is *text input* to ensure keystroke is accepted in input field(s),
      // otherwise the state update via setSearch will cause text input to blur and not
      // accepting keystroke input by user.
      if (e.target.nodeName === 'INPUT' && e.target.type === 'text') {
        return
      }

      setSearch((prevSearch) => prevSearch + e.key.toLowerCase())

      if (e.key === ' ') {
        e.preventDefault()
      }
    } else if (e.key === 'Escape') {
      dropdown.dropdownRef.current.focus()
    }
  }

  useWindowEventListener('keydown', handleKeydown)

  return null
}
