import { styled } from 'twin.macro'

export default styled.button({
  color: 'inherit',
  margin: 0,
  padding: 0,
  border: 'none',
  background: 'none',

  // While I don't want this in here because it is technically styling,
  // I'm waiting on the results of the design team's decision to make
  // all buttons in the app have pointers. Then this should be moved to
  // global css.
  cursor: 'pointer',
})
