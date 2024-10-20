/**
 * Will convert the first character of a string to upper case,
 * and keep the rest of the string as is.
 *
 * @example
 * capitalizeFirstChar({ text: 'hello world' }) // Hello world
 * capitalizeFirstChar({ text: 'open Portfolio page' }) // Open Portfolio page
 * capitalizeFirstChar({ text: ' not trimmed' }) // ' not trimmed'
 */
const capitalizeFirstChar = (text: string): string => {
  if (text === '') {
    return text
  }

  return text.charAt(0).toUpperCase() + text.slice(1)
}

export default capitalizeFirstChar
