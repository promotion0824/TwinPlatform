import { useEffect } from 'react'
import { useTypeahead } from './TypeaheadContext'

export default function ScrollToSelectedOption() {
  const typeahead = useTypeahead()

  useEffect(() => {
    const contentElement = typeahead.contentRef.current.childNodes[0]
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
