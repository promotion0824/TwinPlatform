export default {
  get(key) {
    try {
      return JSON.parse(window.localStorage.getItem(key))
    } catch (err) {
      return null
    }
  },

  set(key, json) {
    try {
      window.localStorage.setItem(key, JSON.stringify(json))

      return true
    } catch (err) {
      return false
    }
  },
}
