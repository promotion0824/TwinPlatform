import tw, { styled } from 'twin.macro'

const verticalSpanVariant = {
  spanOne: tw`row-end-3`,
  spanTwo: tw`row-end-4`,
  spanThree: tw`row-end-5`,
}
const borderVariant = {
  border: tw`border border-gray-550 border-solid`,
  none: '',
}
const horizontalVariant = {
  left: tw`col-start-1 col-end-2`,
  right: tw`col-start-2 col-end-3`,
}
const verticalVariant = {
  top: tw`row-start-2`,
  mid: tw`row-start-3`,
  bot: tw`row-start-4`,
}

const StyledColumnItemContainer = styled.div(() => [
  ({ horizontal }) => horizontalVariant[horizontal],
  ({ vertical }) => verticalVariant[vertical],
  ({ verticalSpan }) => verticalSpanVariant[verticalSpan],
  ({ border }) => borderVariant[border],
])

export default function ColumnItemContainer({
  horizontal = 'left',
  vertical = 'top',
  verticalSpan = 'spanOne',
  border = 'border',
  children,
}) {
  return (
    <StyledColumnItemContainer
      horizontal={horizontal}
      vertical={vertical}
      verticalSpan={verticalSpan}
      border={border}
    >
      {children}
    </StyledColumnItemContainer>
  )
}
