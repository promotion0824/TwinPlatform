/* eslint-disable import/prefer-default-export */
import { styled } from 'twin.macro'

export const MsgContainer = styled.div<{
  $isCopilot?: boolean
}>(({ theme, $isCopilot = false }) => ({
  width: '430px',
  padding: theme.spacing.s16,
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  alignItems: 'center',
  gap: theme.spacing.s8,
  borderRadius: '4px',
  // TODO: Spike to replace bg colors with tokens (https://dev.azure.com/willowdev/Unified/_workitems/edit/116402)
  background: $isCopilot ? '#474747' : '#4B495A',
  margin: '8px auto',
}))
