export default function caseInsensitiveEquals(str1, str2) {
  if (str1 == null) {
    return false
  }

  return str1.localeCompare(str2, undefined, { sensitivity: 'base' }) === 0
}
