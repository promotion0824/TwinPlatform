import styled, { css } from 'styled-components'

interface DataGridOverlayProps {
  children?: string | JSX.Element
}

const DataGridOverlay = ({ children }: DataGridOverlayProps) => {
  return <OverlayWrapper>{children}</OverlayWrapper>
}

const OverlayWrapper = styled.div`
  width: 100%;
  height: 100%;
  display: flex;
  align-self: center;
  align-items: center;
  justify-content: center;

  ${css(({ theme }) => theme.font.heading.lg)}
  color: ${({ theme }) => theme.color.neutral.fg.muted}
`

export default DataGridOverlay
