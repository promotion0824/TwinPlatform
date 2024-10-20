import { useEffect } from 'react'
import { useDropdown } from 'components/Dropdown/Dropdown'

export default function ScrollToSelectedOption() {
  const dropdown = useDropdown()

  useEffect(() => {
    const contentElement = dropdown.contentRef.current.childNodes[0]
    const selectedElement = contentElement.querySelector(
      '[data-is-selected=true]'
    )

    if (selectedElement != null) {
      contentElement.scrollTop =
        selectedElement.offsetTop +
        selectedElement.offsetHeight -
        contentElement.offsetHeight
    }
  }, [])

  return null
}
