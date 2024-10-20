import { useLayoutEffect, useRef, useState } from 'react'
import cx from 'classnames'
import { Flex, Text, Time } from '@willow/ui'
import { useGraph } from '../GraphContext'
import Name from '../Name/Name'
import styles from './Tooltip.css'

export default function Tooltip({ selected }) {
  const graph = useGraph()

  const tooltipRef = useRef()
  const [state, setState] = useState({
    position: undefined,
    style: undefined,
  })

  useLayoutEffect(() => {
    let position = 'right'
    let left = selected.column.left + selected.column.width / 2
    if (
      left + tooltipRef.current.offsetWidth >
      graph.svgRef.current.clientWidth
    ) {
      left =
        selected.column.left -
        selected.column.width / 2 -
        tooltipRef.current.offsetWidth
      position = 'left'
    }

    const top =
      graph.svgRef.current.clientHeight -
      selected.segment.y -
      selected.segment.height / 2 -
      tooltipRef.current.offsetHeight / 2

    setState({
      position,
      style: { left, top },
    })
  }, [])

  const cxClassName = cx(styles.tooltip, {
    [styles.positionLeft]: state.position === 'left',
    [styles.positionRight]: state.position === 'right',
  })

  return (
    <div ref={tooltipRef} className={cxClassName} style={state.style}>
      <Flex size="medium" padding="large">
        <Text type="h3">
          <Time value={selected.column.name} format="date" />
        </Text>
        <Name name={selected.segment.name} />
        <Text type="h2">{selected.segment.value}</Text>
      </Flex>
    </div>
  )
}
