import { useState } from 'react'
import { useWindowEventListener } from 'hooks'
import Tooltip from 'components/Tooltip/Tooltip'

function findTooltipElement(node) {
  if (!node || !node.getAttribute) {
    return undefined
  }

  if (node.getAttribute('data-tooltip') != null) {
    return node
  }

  return findTooltipElement(node.parentNode)
}

export default function TooltipProvider(props) {
  const { children } = props

  const [tooltip, setTooltip] = useState()

  useWindowEventListener('mousemove', (e) => {
    const element = findTooltipElement(e.target)
    if (element != null) {
      setTooltip({
        target: element,
        title: element.getAttribute('data-tooltip'),
      })
    } else {
      setTooltip()
    }
  })

  useWindowEventListener('wheel', () => {
    setTooltip()
  })

  return (
    <>
      {children}
      {tooltip && <Tooltip target={tooltip.target}>{tooltip.title}</Tooltip>}
    </>
  )
}
