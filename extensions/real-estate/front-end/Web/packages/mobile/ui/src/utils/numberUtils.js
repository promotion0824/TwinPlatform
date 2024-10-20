import numeral from 'numeral'

export default {
  parse(str, format, options = {}) {
    let number = numeral(str).value()
    if (Number.isNaN(number)) number = null
    if (!/^[0-9., -]+$/.test(str)) number = null
    if (number == null) return null

    if (options.max != null) number = Math.min(number, options.max)
    if (options.min != null) number = Math.max(number, options.min)

    return format == null
      ? number
      : numeral(numeral(number).format(format)).value()
  },

  format(number, format) {
    if (number == null) return ''
    if (number === Infinity) return ''
    if (number === -Infinity) return ''

    const value = numeral(number).value()
    if (value == null) return ''
    if (format == null) return `${value}`

    return numeral(number).format(format)
  },
}
