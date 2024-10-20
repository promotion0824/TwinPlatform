export default function getValue(value) {
  if (value == null) {
    return null
  }

  if (new Date(value) !== 'Invalid Date') {
    return new Date(value).valueOf()
  }

  return value
}
