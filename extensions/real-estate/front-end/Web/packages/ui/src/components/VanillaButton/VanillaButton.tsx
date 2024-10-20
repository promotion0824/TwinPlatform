import styled from 'styled-components'

/**
 * This is a temporary solution for a vanilla button without any
 * style. We might provide one from the @willowinc/ui in the future.
 */
const VanillaButton = styled.button`
  background: none;
  color: inherit;
  border: none;
  padding: 0;
  font: inherit;
  cursor: pointer;
  outline: inherit;
`

export default VanillaButton
