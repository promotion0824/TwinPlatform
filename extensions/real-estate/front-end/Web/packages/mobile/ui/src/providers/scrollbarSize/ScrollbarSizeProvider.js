import { useState } from 'react'
import { ScrollbarSizeContext } from './ScrollbarSizeContext'
import ScrollbarSize from './ScrollbarSize'

export default function ScrollbarSizeProvider(props) {
  const { children } = props

  const [scrollbarSize, setScrollbarSize] = useState({})

  function handleScrollbarSizeChange(size) {
    setScrollbarSize(size)
  }

  return (
    <ScrollbarSizeContext.Provider {...props} value={scrollbarSize}>
      {children}
      <ScrollbarSize onChange={handleScrollbarSizeChange} />
    </ScrollbarSizeContext.Provider>
  )
}
