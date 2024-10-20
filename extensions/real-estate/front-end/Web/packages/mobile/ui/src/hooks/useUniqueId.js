import { useRef } from 'react'
import _ from 'lodash'

export default function useUniqueId() {
  const uniqueIdRef = useRef(_.uniqueId())

  return uniqueIdRef.current
}
