import { Flex, Time } from '@willow/ui'
import { styled } from 'twin.macro'
import { Timestamp } from '../types'

export default function Name({ timestamp }: { timestamp: Timestamp }) {
  return (
    <Flex horizontal align="middle" size="small">
      <DarkGreenCircle />
      <Time value={timestamp} />
    </Flex>
  )
}

const DarkGreenCircle = styled.div({
  backgroundColor: '#29372a',
  borderRadius: '100%',
  height: '16px',
  width: '16px',
})
