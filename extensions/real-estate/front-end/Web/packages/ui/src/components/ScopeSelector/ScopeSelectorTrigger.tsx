import { forwardRef } from 'react'
import styled from 'styled-components'
import { Button, Icon, IconName } from '@willowinc/ui'

import { LocationNode } from './ScopeSelector'
import ScopeSelectorAvatar from './ScopeSelectorAvatars'

const ButtonContent = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s8,
}))

const LocationName = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
}))

const StyledButton = styled(Button)<{ $isOpen: boolean }>(
  ({ $isOpen, theme }) => ({
    outline: 'none',
    padding: theme.spacing.s4,

    ...($isOpen && {
      backgroundColor: theme.color.neutral.bg.panel.activated,
    }),
  })
)

type ScopeSelectorTriggerProps = {
  isOpen: boolean
  // onClick is provided by Popover.Target
  onClick?: () => void
  selectedLocation: LocationNode
}

export default forwardRef<HTMLButtonElement, ScopeSelectorTriggerProps>(
  function ScopeSelectorTrigger({ isOpen, onClick, selectedLocation }, ref) {
    return (
      <StyledButton
        $isOpen={isOpen}
        kind="secondary"
        onClick={onClick}
        ref={ref}
      >
        <ButtonContent>
          <ScopeSelectorAvatar
            modelId={selectedLocation.twin.metadata.modelId}
          />

          <LocationName>{selectedLocation?.twin.name}</LocationName>

          <Icon icon={(isOpen ? 'expand_less' : 'expand_more') as IconName} />
        </ButtonContent>
      </StyledButton>
    )
  }
)
