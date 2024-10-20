import numberUtils from './numberUtils'

const cookieUtils = {
  parse() {
    return document.cookie
      .split(';')
      .map((cookie) => cookie.split('=').map((str) => str.trim()))
      .filter((pair) => pair.length === 2)
      .map((pair) => {
        const [key, value] = pair

        const number = numberUtils.parse(value)
        let nextValue = value
        if (number != null) nextValue = number
        if (value.toLowerCase() === 'true') nextValue = true
        if (value.toLowerCase() === 'false') nextValue = false

        return [key, nextValue]
      })
      .reduce(
        (acc, arr) => ({
          ...acc,
          [arr[0]]: arr[1],
        }),
        {}
      )
  },

  get(cookie) {
    return cookieUtils.parse()[cookie]
  },

  set(cookie, value) {
    if (value == null) {
      // delete cookie
      document.cookie = `${cookie}=;path=/;expires=Thu, 01 Jan 1970 00:00:00 UTC;`
      return
    }

    // set cookie to maximum expiry date
    document.cookie = `${cookie}=${value};path=/;expires=Tue, 19 Jan 2038 03:14:07 UTC;`
  },
}

export default cookieUtils
