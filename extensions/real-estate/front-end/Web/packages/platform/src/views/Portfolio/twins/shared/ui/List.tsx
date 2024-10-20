import { styled } from 'twin.macro'

const List = styled.ul({
  width: '100%',
  margin: 0,
  padding: 0,
  listStyleType: 'none',

  '& > li': {
    borderBottom: 'solid 1px #383838',
  },
  '& > li:last-of-type': {
    borderBottom: 'none',
  },
})

export default List
