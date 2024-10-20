import { useState } from 'react'
import _ from 'lodash'

export default function useUniqueId() {
  const [uniqueId] = useState(() => _.uniqueId())

  return uniqueId
}
