export function stringToObject<T>(str: string | null): T | null {
  if (!str) return null

  try {
    return JSON.parse(str)
  } catch (error) {
    console.error('JSON parsing error:', error)
    return null
  }
}
