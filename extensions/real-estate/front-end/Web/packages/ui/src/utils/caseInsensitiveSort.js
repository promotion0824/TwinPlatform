export default function caseInsensitiveSort(fn) {
  return (a, b) => {
    const str1 = fn(a)
    const str2 = fn(b)

    // preserve original order when both are undefined/null
    if (str1 == null && str2 == null) return 0

    // a string is always sorted before a null/undefined
    if (str1 == null && str2 != null) return 1
    if (str1 != null && str2 == null) return -1

    return str1.localeCompare(str2, undefined, { sensitivity: 'base' })
  }
}
