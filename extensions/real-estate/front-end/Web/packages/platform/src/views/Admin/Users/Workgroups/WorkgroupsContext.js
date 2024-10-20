import { createContext, useContext } from 'react'

export const WorkgroupsContext = createContext()

export function useWorkgroups() {
  return useContext(WorkgroupsContext)
}
