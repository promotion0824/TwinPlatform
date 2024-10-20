import styled from 'styled-components'
import { v4 as uuidv4 } from 'uuid'

const getContainmentHelper = (containerName?: string) => {
  const uniqueContainerName =
    containerName ?? `a${uuidv4()}` /* cannot start with digit */

  const getContainerQuery = (rule: string) =>
    `@container ${uniqueContainerName} (${rule})`

  return {
    ContainmentWrapper: styled.div({
      containerType: 'size',
      containerName: uniqueContainerName,
      height: '100%',
      overflowY: 'auto',
    }),

    getContainerQuery,
    containerName: uniqueContainerName,
  }
}

export default getContainmentHelper
