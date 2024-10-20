import { useState, PropsWithChildren } from 'react'
import { Button, Flex, Text, Icon, useUser, useAnalytics } from '@willow/ui'
import { styled } from 'twin.macro'

/**
 * MainMenuButton component displays a navigation button that includes a tile / header / extra description
 * It is mainly used in MainMenu component to provide an access to main pages
 */
type SubmenuProps = PropsWithChildren<{
  header: string
  tile?: string
  isSelected?: boolean
  to?: string
  disabled?: boolean
  target?: string
}>

const MenuContent = ({ tile, header, children, isSelected }) => (
  <Flex horizontal fill="content" size="large" padding="medium large">
    <TileWrapper $isSelected={isSelected}>
      <Text size="extraLarge">{tile}</Text>
    </TileWrapper>
    <Flex align="middle">
      <Text>{header}</Text>
      <Text color="grey">{children}</Text>
    </Flex>
  </Flex>
)

export default function MainMenuButton({
  header,
  to,
  defaultTile,
  disabled,
  target,
  rel,
  href,
  children,
  submenus = [],
  isSelected,
  'data-testid': dataTestId,
  onClick,
}: PropsWithChildren<{
  header: string
  to?: string
  defaultTile?: string // define a custom tile instead of auto-generation text
  disabled?: boolean
  target?: string
  rel?: string
  href?: string
  submenus?: SubmenuProps[]
  isSelected?: boolean
  'data-testid'?: string
  onClick: () => void
}>) {
  const [submenuExpanded, setSubmenuExpanded] = useState(false)
  const user = useUser()
  const analytics = useAnalytics()

  const getTileInitial = (headerStr: string, defaultTileStr?: string) =>
    defaultTileStr ??
    headerStr
      .split(' ')
      .map((word) => word[0])
      .join('')
      .toUpperCase()

  const toggleSubmenu = (event) => {
    event.stopPropagation()
    event.preventDefault()
    setSubmenuExpanded(!submenuExpanded)
  }

  return (
    <>
      <MenuButton
        width="100%"
        onClick={() => {
          analytics?.track('Main Menu Clicked', {
            main_menu_button_name: header,
            customer_name: user?.customer?.name ?? '',
          })

          onClick()
        }}
        to={to}
        disabled={disabled}
        target={target}
        rel={rel}
        href={href}
        $isSelected={isSelected}
        data-testid={dataTestId}
      >
        <FlexContainer>
          <MenuContent
            tile={getTileInitial(header, defaultTile)}
            header={header}
            isSelected={isSelected}
          >
            {children}
          </MenuContent>
          {submenus.length > 0 && !disabled && (
            <SubmenuIcon
              icon="chevron"
              $isSelected={submenuExpanded}
              onClick={toggleSubmenu}
              padding="medium"
            />
          )}
        </FlexContainer>
      </MenuButton>
      {submenuExpanded &&
        submenus.map((submenu: SubmenuProps) => (
          <MenuButton
            to={submenu.to}
            disabled={submenu.disabled}
            target={submenu.target}
            width="100%"
            key={submenu.header}
            $isSelected={submenu.isSelected}
            $isSubmenu
            onClick={onClick}
          >
            <MenuContent
              tile={getTileInitial(submenu.header)}
              header={submenu.header}
              isSelected={submenu.isSelected}
            >
              {submenu.children}
            </MenuContent>
          </MenuButton>
        ))}
    </>
  )
}

const TileWrapper = styled.div<{ $isSelected: boolean }>`
  display: flex;
  flex-flow: column;
  text-align: center;
  justify-content: center;
  border-radius: var(--border-radius-large);
  height: 40px;
  transition: all 0.2s ease;
  width: 40px;
  background-color: ${(props) =>
    props.$isSelected ? 'var(--purple)' : 'none'};
`

const MenuButton = styled(Button)<{
  $isSubmenu?: boolean
  $isSelected?: boolean
}>`
  &:focus ${TileWrapper} {
    background-color: var(--purple);
    color: var(--white);
  }

  @media (hover: hover) {
    &:hover ${TileWrapper} {
      background-color: ${(props) =>
        props.disabled ? 'none' : 'var(--purple)'};
      color: ${(props) => (props.disabled ? 'none' : 'var(--white)')};
    }
  }
  opacity: ${(props) => (props.disabled ? 0.4 : 'initial')};
  color: ${(props) =>
    props.$isSelected ? 'var(--white)' : 'var(--text) !important'};
  padding-left: ${(props) => (props.$isSubmenu ? '40px' : '0px')};
`

const SubmenuIcon = styled(Icon)<{ $isSelected: boolean }>`
  cursor: pointer;
  margin-top: 5px;
  margin-left: 15px;
  transform: ${(props) =>
    props.$isSelected ? 'rotate(180deg)' : 'rotate(0deg)'};
`

const FlexContainer = styled.div`
  display: flex;
  align-items: center;
`
