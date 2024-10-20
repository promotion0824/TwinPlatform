import { cookie } from '@willow/ui'

export default function getFeatures(features) {
  const nextFeatures = Object.fromEntries(
    Object.entries(features).map(([key, value]) => {
      const cookieKey = `wp-${key}`
      const cookieValue = cookie.get(cookieKey)

      return [key, cookieValue ?? value]
    })
  )

  return nextFeatures
}
