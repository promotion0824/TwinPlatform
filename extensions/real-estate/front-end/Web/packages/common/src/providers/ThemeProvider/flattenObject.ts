type InputObject = { [key: string]: string | number | InputObject }

/**
 * Flatten a theme object including any nested object. The key of the flatten object
 * consist of: the prefix, the key & any nested keys seperated by a separator.
 * If excludeDefault flag is true, we omit the word 'default' for the key.
 *
 * For example, given the input object with excludeDefault `true`
 * {
 *   bg: {
 *     state: {
 *       default: gray,
 *       hovered: gray-hovered,
 *       active: gray-active
 *     }
 *   }
 * }
 *
 * The object will be flattened to:
 * {
 *   bg.state: gray,
 *   bg.state.hovered: gray-hovered,
 *   bg.state.active: gray-active
 * }
 *
 * @param ob The theme object to be flattened
 * @param sep Separator between the keys of nested object(s)
 * @param keyPrefix The prefix for flattened object key
 * @param excludeDefault Whether to exclude the word "default" in the key of flattened object
 * @returns The flattened object
 */
export default function flattenObject(
  ob: InputObject,
  sep = '.',
  keyPrefix = '',
  excludeDefault = false
) {
  let flattenedOb: { [key: string]: string | number } = {}
  const prefix = `${keyPrefix}`

  for (const key of Object.keys(ob)) {
    if (typeof ob[key] === 'object' && ob[key] != null) {
      const subOb = flattenObject(
        ob[key] as InputObject,
        sep,
        `${prefix}${key}${sep}`,
        excludeDefault
      )
      flattenedOb = Object.assign(flattenedOb, subOb)
    } else {
      let concatKey
      if (excludeDefault && key === 'default') {
        if (prefix.endsWith(sep)) {
          concatKey = prefix.slice(0, -sep.length)
        } else {
          concatKey = prefix
        }
      } else {
        concatKey = `${prefix}${key}`
      }
      flattenedOb[concatKey] = `${ob[key]}`
    }
  }

  return flattenedOb
}
