import styled from 'styled-components'

export const ChartContainer = styled.div({
  height: '600px',
})

export const ChartContainerDecorator = (Story: React.ComponentType) => (
  <ChartContainer>
    <Story />
  </ChartContainer>
)
