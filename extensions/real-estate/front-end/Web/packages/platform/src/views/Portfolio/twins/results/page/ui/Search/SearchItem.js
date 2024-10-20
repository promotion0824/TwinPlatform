import { styled } from 'twin.macro'
import { Icon } from '@willowinc/ui'

const Li = styled.li({
  margin: '0.5rem 0',
  paddingLeft: ({ indented }) => (indented ? '1rem' : 0),

  listStyle: 'none',
})

const Button = styled.button(({ isSelected, theme }) => ({
  margin: 0,
  padding: 0,

  width: '100%',
  display: 'flex',
  justifyContent: 'space-between',
  textAlign: 'left',

  background: 'none',
  border: 'none',

  color: isSelected
    ? theme.color.neutral.fg.default
    : theme.color.neutral.fg.muted,
  ...theme.font.body.md.regular,

  cursor: 'pointer',

  '&:hover': {
    color: theme.color.neutral.fg.default,
  },
}))

const StyledIcon = styled(Icon)(({ theme }) => ({
  color: theme.color.intent.primary.fg.default,
}))

const SearchItem = ({ children, indented, isSelected, onClick }) => (
  <Li indented={indented}>
    <Button
      isSelected={isSelected}
      onClick={onClick}
      data-testid={`search-item-${children}`}
    >
      <div>{children}</div>
      {isSelected && <StyledIcon icon="check" />}
    </Button>
  </Li>
)

export default SearchItem
