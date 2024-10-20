import { useEffect, useState } from 'react'
import Number from 'components/Number/Number'
import { useTimer } from '@willow/ui'

export default function AnimatedNumber({ value }) {
  const timer = useTimer()

  const [displayedValue, setDisplayedValue] = useState(0)

  useEffect(() => {
    const valueCount = 20
    const values = [
      ...Array.from(Array(valueCount)).map(
        (n, i) => (value - 0) * (1 / valueCount) * i
      ),
      value,
    ]

    async function loadValue(nextValue) {
      await timer.sleep(200 / valueCount)

      setDisplayedValue(nextValue)
    }

    async function load() {
      for (let i = 0; i < values.length; i++) {
        await loadValue(values[i]) // eslint-disable-line
      }
    }

    load()
  }, [value])

  return <Number value={displayedValue} format="%" invalid="-" />
}
