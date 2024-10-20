import styled from 'styled-components'

export const FlexContainer = styled.div<{
  flexFlow: string
  flexNumber: string
  isInline?: boolean
  width?: string
}>(({ flexFlow, flexNumber, isInline = false, width }) => ({
  flexFlow,
  flexShrink: 0,
  display: isInline ? 'inline-block' : 'flex',
  flex: flexNumber,
  flexWrap: 'wrap',
  width,
}))

export const GridContainer = styled.div<{ index: number }>(({ index }) => ({
  width: '49%',
  display: 'inline-grid',
  paddingLeft: index % 2 ? '16px' : '0',
}))
