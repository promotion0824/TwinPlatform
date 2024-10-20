/* eslint-disable no-bitwise */

export default {
  hashCode(str) {
    let hash = 0

    if (str.length === 0) return hash

    for (let i = 0; i < str.length; i++) {
      const chr = str.charCodeAt(i)
      hash = (hash << 5) - hash + chr
      hash |= 0 // Convert to 32bit integer
    }

    return hash
  },

  isNullOrEmpty(str) {
    return !str || /^\s*$/.test(str)
  },

  capitalizeFirstLetter(str) {
    return str.charAt(0).toUpperCase() + str.slice(1)
  },
}
