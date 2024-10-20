import { useRef } from 'react'
import cx from 'classnames'
import { Button, Flex } from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import Drag from './Drag/Drag'
import styles from './Range.css'

export default function Range({
  value = 0,
  values,
  className,
  onChange = () => {},
}) {
  const trackRef = useRef()

  const valuesWithPercentages = values.map((derivedValue, i) => ({
    value: derivedValue,
    percentage: (i * 100) / (values.length - 1),
  }))

  const percentage =
    valuesWithPercentages.find(
      (valueWithPercentage) => valueWithPercentage.value === value
    )?.percentage ?? 0

  const cxClassName = cx(styles.range, className)

  function handleDown(drag) {
    return {
      ...drag,
      startTrackX: trackRef.current.getBoundingClientRect().left,
    }
  }

  function handleMove(drag) {
    const dragPercentage =
      ((drag.clientX - drag.startTrackX) * 100) / trackRef.current.offsetWidth

    const percentageRanges = valuesWithPercentages.map((current, i) => {
      const prev = valuesWithPercentages[i - 1]
      const next = valuesWithPercentages[i + 1]

      return {
        ...current,
        minPercentage:
          prev != null
            ? current.percentage + (prev.percentage - current.percentage) / 2
            : current.percentage,
        maxPercentage:
          next != null
            ? current.percentage + (next.percentage - current.percentage) / 2
            : current.percentage,
      }
    })

    let range = percentageRanges.find(
      (percentageRange) =>
        dragPercentage >= percentageRange.minPercentage &&
        dragPercentage <= percentageRange.maxPercentage
    )
    if (range == null) {
      range =
        dragPercentage < 0 ? percentageRanges[0] : percentageRanges.slice(-1)[0]
    }

    onChange(range.value)
  }

  function handleMinusClick() {
    const nextValue = [...values].reverse().find((v) => v < value) ?? values[0]

    onChange(nextValue)
  }

  function handlePlusClick() {
    const nextValue = values.find((v) => v > value) ?? values.slice(-1)[0]

    onChange(nextValue)
  }

  return (
    <Flex horizontal size="small" className={cxClassName}>
      <IconButton
        icon="remove"
        kind="secondary"
        background="transparent"
        disabled={value === values[0]}
        onClick={handleMinusClick}
      />
      <Drag
        moveOnDown
        onDown={handleDown}
        onMove={handleMove}
        className={styles.drag}
      >
        {(drag) => (
          <div
            ref={trackRef}
            className={styles.track}
            onPointerDown={drag.onPointerDown}
          >
            <div className={styles.trackCenter}>
              <div
                className={styles.highlightedTrack}
                style={{
                  width: `${percentage}%`,
                }}
              />
              <Button
                className={styles.point}
                style={{
                  left: `${percentage}%`,
                }}
              />
            </div>
          </div>
        )}
      </Drag>
      <IconButton
        icon="add"
        kind="secondary"
        background="transparent"
        disabled={value === values.slice(-1)[0]}
        onClick={handlePlusClick}
      />
    </Flex>
  )
}
