import styled from 'styled-components'

const EditingModeOverlay = styled.div<{ $isEditingMode: boolean }>(
  ({ $isEditingMode }) => ({
    width: '100%',
    ...($isEditingMode && {
      opacity: 0.3,
      pointerEvents: 'none',
      '-webkit-user-select': 'none' /* Safari */,
      '-ms-user-select': 'none' /* IE 10 and IE 11 */,
      'user-select': 'none' /* Standard syntax */,
    }),
  })
)

export default EditingModeOverlay
