import { useEffect } from 'react'
import { useDropdown } from 'components/Dropdown/Dropdown'

export default function ScrollToSelectedOption() {
  const dropdown = useDropdown()

  useEffect(() => {
    const selected = dropdown.contentRef.current.querySelector(
      '[data-is-selected=true]'
    )
    if (selected) {
      dropdown.contentRef.current.scrollTop =
        selected.offsetTop +
        selected.offsetHeight -
        dropdown.contentRef.current.offsetHeight
    }
  }, [])

  return null
}
