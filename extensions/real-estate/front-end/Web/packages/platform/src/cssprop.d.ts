import { CSSProp } from 'styled-components'

// fix for https://github.com/DefinitelyTyped/DefinitelyTyped/issues/31245#issuecomment-446011384
declare module 'react' {
  interface DOMAttributes<T> {
    css?: CSSProp
  }
}
declare global {
  namespace JSX {
    interface IntrinsicAttributes {
      css?: CSSProp
    }
  }
}
