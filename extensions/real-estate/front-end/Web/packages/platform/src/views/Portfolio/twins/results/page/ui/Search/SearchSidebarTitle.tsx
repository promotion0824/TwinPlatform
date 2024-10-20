import { styled } from 'twin.macro'

const SearchSidebarTitle = styled.div({
  display: 'flex',
  alignItems: 'center',

  textTransform: 'uppercase',
  fontSize: '10px',
  fontWeight: 'bold',

  marginTop: '2rem',

  '&:first-child': {
    marginTop: '0',
  },
})

export default SearchSidebarTitle
