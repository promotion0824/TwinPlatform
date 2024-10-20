import { useDateTime } from 'hooks'
import Select, { Option } from 'components/Select/Select'

export default function TimePicker({ min, max, value, onChange, ...rest }) {
  const dateTime = useDateTime()

  const times = []
  if (value != null) {
    const startTime = dateTime(value).startOfDay()
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
      value={dateTime(value).format()}
      header={(time) => dateTime(time).format('time')}
      onChange={(time) => onChange(time)}
    >
      {times.map((time) => {
        const isDisabled =
          (min != null && time <= dateTime(min)) ||
          (max != null && time >= dateTime(max))

        return (
          <Option
            key={time}
            value={dateTime(time).format()}
            disabled={isDisabled}
          >
            {time.format('time')}
          </Option>
        )
      })}
    </Select>
  )
}
