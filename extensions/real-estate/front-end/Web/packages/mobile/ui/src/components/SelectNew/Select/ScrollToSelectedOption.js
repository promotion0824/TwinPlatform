import { useEffect } from 'react'
import { useDropdown } from 'components/DropdownNew/Dropdown'

export default function ScrollToSelectedOption() {
  const dropdown = useDropdown()

  useEffect(() => {
    const content = dropdown.contentRef.current.childNodes[0]
    const selected = content.querySelector('[data-is-selected=true]')

    if (selected) {
      content.scrollTop =
        selected.offsetTop + selected.offsetHeight - content.offsetHeight
    }
  }, [])

  return null
}
