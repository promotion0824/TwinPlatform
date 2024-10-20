import { useRef, useState, useEffect } from 'react'
import { useHistory } from 'react-router'
import { useWindowEventListener } from '@willow/ui'
import Tooltip from 'components/Tooltip/Tooltip'
import { styled } from 'twin.macro'

function findTooltipElement(node) {
  if (!node || !node.getAttribute) {
    return undefined
  }

  if (node.getAttribute('data-tooltip') != null) {
    return node
  }

  return findTooltipElement(node.parentNode)
}

// eslint-disable-next-line
export function TooltipProvider({ children }) {
  const [tooltip, setTooltip] = useState()
  const timeoutIdRef = useRef()
  const innerHTMLRef = useRef()
  const history = useHistory()

  // many elements that we display tooltip for are either a link
  // or will direct user to another path, so we need to clear
  // tooltip when it happens
  useEffect(() => {
    const unlisten = history.listen(() => {
      setTooltip()
    })

    return () => {
      unlisten()
    }
  }, [history])

  useWindowEventListener('mousemove', (e) => {
    const element = findTooltipElement(e.target)
    const tooltipDelayTime = element?.getAttribute('data-tooltip-time')
    const isSameElement = element?.innerHTML === innerHTMLRef.current
    const clearTooltip = () => {
      clearTimeout(timeoutIdRef.current)
      timeoutIdRef.current = null
      setTooltip()
    }

    // resets everything when mouse is not hovering over an element
    // that has "data-tooltip" props
    if (element == null) {
      clearTooltip()
      return
    }

    // do nothing when mouse is hovering over the same element
    // and a tooltip is already displayed
    if (tooltip != null && isSameElement) {
      return
    }

    // sets tooltip immediately when mouse is hovering over
    // an element and there is no delay required
    if (!tooltipDelayTime) {
      setTooltip(getTooltipProps(element))
      return
    }

    // resets everything when user mouses over a different element
    if (!isSameElement) {
      clearTooltip()
      innerHTMLRef.current = element?.innerHTML
      return
    }

    // Showing the tooltip after a delay based on data-tooltip-time prop which is in milliseconds
    if (tooltipDelayTime && isSameElement && timeoutIdRef.current == null) {
      timeoutIdRef.current = setTimeout(() => {
        setTooltip(getTooltipProps(element))
      }, tooltipDelayTime)
    }
  })

  useWindowEventListener('wheel', () => {
    setTooltip()
  })

  return (
    <>
      {children}
      {tooltip && (
        <StyledTooltip
          target={tooltip.target}
          position={tooltip.position}
          paddingTop={tooltip.paddingTop}
          $width={tooltip.width}
          animate={tooltip.animate !== 'false'}
          zIndex={tooltip.zIndex}
        >
          {tooltip.title}
        </StyledTooltip>
      )}
    </>
  )
}

// Setting width of the tooltip based on data-tooltip-width prop
const StyledTooltip = styled(Tooltip)(({ $width }) => ({
  width: $width,
}))

const getTooltipProps = (element) => ({
  target: element,
  title: element.getAttribute('data-tooltip'),
  position: element.getAttribute('data-tooltip-position'),
  paddingTop: element.getAttribute('data-tooltip-paddingtop'),
  width: element.getAttribute('data-tooltip-width'),
  animate: element.getAttribute('data-tooltip-animate'),
  zIndex: element.getAttribute('data-tooltip-z-index'),
})
