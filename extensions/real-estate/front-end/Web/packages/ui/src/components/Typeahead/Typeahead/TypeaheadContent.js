import { useLayoutEffect, useState } from 'react'
import cx from 'classnames'
import { useSize, useWindowEventListener } from '@willow/ui'
import OnClickOutside from 'components/OnClickOutside/OnClickOutside'
import KeyboardHandler from './KeyboardHandler'
import ScrollToSelectedOption from './ScrollToSelectedOption'
import { useTypeahead } from './TypeaheadContext'
import styles from './TypeaheadContent.css'

export default function TypeaheadContent({ children, zIndex }) {
  const typeahead = useTypeahead()

  const contentSize = useSize(typeahead.contentRef)

  const [state, setState] = useState({
    style: undefined,
    isTop: true,
  })

  function refresh() {
    const rect = typeahead.inputRef.current.getBoundingClientRect()
    const contentRect = typeahead.contentRef.current.getBoundingClientRect()

    const { left } = rect
    const right = rect.right - contentRect.width
    const isLeft = left + contentRect.width <= window.innerWidth || right < 0

    const top = rect.top - contentRect.height
    const bottom = rect.top + rect.height
    const isTop = bottom + contentRect.height > window.innerHeight && top >= 0

    setState({
      style: {
        left: isLeft ? left - 1 : right,
        top: isTop ? top : bottom + 1,
        width: rect.width + 2,
      },
      isTop,
    })
  }

  useLayoutEffect(() => {
    refresh()
  }, [contentSize])

  useWindowEventListener('resize', refresh)

  const cxClassName = cx(
    styles.typeaheadContent,
    {
      [styles.top]: state.isTop,
    },
    'ignore-onclickoutside'
  )

  return (
    <OnClickOutside
      targetRefs={[typeahead.inputRef, typeahead.contentRef]}
      closeOnWheel
      onClose={() => typeahead.close()}
    >
      <div
        ref={typeahead.contentRef}
        className={cxClassName}
        css={zIndex ? { zIndex } : {}}
        style={state.style}
      >
        <div className={styles.content}>
          {children}
          <KeyboardHandler />
          <ScrollToSelectedOption />
        </div>
      </div>
    </OnClickOutside>
  )
}
