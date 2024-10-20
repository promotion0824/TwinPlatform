import numeral from 'numeral'

export default {
  parse(str, format, options = {}) {
    let number = numeral(str).value()
    if (Number.isNaN(number)) number = null
    if (!/^[0-9., -]+$/.test(str)) number = null
    if (number == null) return null

    if (options.max != null && number > options.max) {
      number = options.max
    }

    if (options.min != null && number < options.min) {
      number = options.min
    }

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

    const nextNumber = number >= 0 && number <= 0.000001 ? 0 : number

    return numeral(nextNumber).format(format)
  },
}
