import { useState } from 'react'
import { useEffectOnceMounted } from '@willow/common'
import { useUser } from './UserContext'

export default function useUserState(key, value) {
  const user = useUser()

  const [state, setState] = useState(user.options[key] ?? value)

  useEffectOnceMounted(() => {
    user.saveUserOptions(key, state)
  }, [state])

  return [state, setState]
}
