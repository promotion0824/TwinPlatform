import { Tooltip, TooltipProps } from '@willowinc/ui'
import {
  ReactElement,
  cloneElement,
  useCallback,
  useRef,
  useState,
} from 'react'

/**
 * Triggers a tooltip when hovering over a target child component
 * when the text for the target child is truncated.
 *
 * It expects to have a single child component to be able to bind the ref,
 * and requires the target child to be able to accept a ref prop.
 */
const TooltipWhenTruncated = ({
  label,
  children,
  ...rest
}: {
  /** If not provided, will default as the content of the children component */
  label?: string
  children: ReactElement
} & Partial<TooltipProps>) => {
  const targetRef = useRef(null)
  const [opened, setOpened] = useState(false)

  const handleMouseEnter = useCallback(() => {
    if (isTextTruncated(targetRef?.current)) {
      setOpened(true)
    } else {
      setOpened(false)
    }
  }, [])

  const handleMouseLeave = useCallback(() => {
    setOpened(false)
  }, [])

  return (
    <Tooltip
      withinPortal
      label={label ?? children.props.children}
      opened={opened}
      {...rest}
    >
      {cloneElement(children, {
        ref: targetRef,
        onMouseEnter: handleMouseEnter,
        onMouseLeave: handleMouseLeave,
      })}
    </Tooltip>
  )
}

export default TooltipWhenTruncated

/**
 * Checks if an HTML element's text is truncated by CSS.
 */
function isTextTruncated(element: HTMLElement | null) {
  if (!element) {
    return false
  }

  return element.scrollWidth > element.offsetWidth
}
