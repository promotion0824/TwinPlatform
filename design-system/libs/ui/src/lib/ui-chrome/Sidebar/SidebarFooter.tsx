import styled from 'styled-components'
import { IconButton } from '../../buttons/Button'
import { Group } from '../../layout/Group'

const Footer = styled(Group)(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderTop: `1px solid ${theme.color.neutral.border.default}`,
  bottom: 0,
  position: 'sticky',
}))

export const SidebarFooter = ({
  isCollapsed,
  onClick,
}: {
  isCollapsed: boolean
  onClick: () => void
}) => {
  return (
    <Footer justify="flex-end" p="s8">
      <IconButton
        background="transparent"
        icon={isCollapsed ? 'last_page' : 'first_page'}
        kind="secondary"
        onClick={onClick}
      />
    </Footer>
  )
}
