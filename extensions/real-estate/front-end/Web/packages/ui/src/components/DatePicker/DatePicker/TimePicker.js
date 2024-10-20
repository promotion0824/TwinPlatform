import { useDateTime } from '@willow/ui'
import Select, { Option } from 'components/Select/Select'

export default function TimePicker({
  timezone,
  min,
  max,
  value,
  onChange,
  ...rest
}) {
  const dateTime = useDateTime()

  const times = []
  if (value != null) {
    const startTime = dateTime(value, timezone).startOfDay()
    let currentTime = startTime
    do {
      times.push(currentTime)
      currentTime = currentTime.addMinutes(15)
    } while (currentTime.day === startTime.day)
  }

  return (
    <Select
      disabled={times.length === 0}
      unselectable
      {...rest}
      align="right"
      value={dateTime(value, timezone).format()}
      header={(time) => dateTime(time, timezone).format('time')}
      onChange={(time) => onChange(time)}
    >
      {times.map((time) => {
        const isDisabled =
          (min != null && time <= dateTime(min, timezone)) ||
          (max != null && time >= dateTime(max, timezone))

        return (
          <Option
            key={time}
            value={dateTime(time, timezone).format()}
            disabled={isDisabled}
          >
            {time.format('time')}
          </Option>
        )
      })}
    </Select>
  )
}
