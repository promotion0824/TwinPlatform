import { useState } from 'react'
import { useTimer, useWindowEventListener } from 'hooks'
import { useDropdown } from 'components/DropdownNew/Dropdown'
import { useEffectOnceMounted } from '@willow/common'

export default function KeyboardHandler() {
  const dropdown = useDropdown()

  const timer = useTimer()
  const [search, setSearch] = useState('')

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

  useEffectOnceMounted(() => {
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

      timer.setTimeout(() => {
        setSearch('')
      }, 500)
    }
  }, [search])

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
      } else if (e.key === 'Tab' && e.shiftKey) {
        up(1)
      } else if (e.key === 'Tab') {
        down(1)
      } else if (e.key === 'PageUp') {
        up(10)
      } else if (e.key === 'PageDown') {
        down(10)
      } else if (e.key.length === 1) {
        setSearch((prevSearch) => prevSearch + e.key.toLowerCase())

        if (e.key === ' ') {
          e.preventDefault()
        }
      }
    }
  })

  return null
}
