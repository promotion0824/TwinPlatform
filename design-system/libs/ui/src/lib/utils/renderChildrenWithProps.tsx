import {
  Attributes,
  Children,
  cloneElement,
  isValidElement,
  ReactElement,
  ReactNode,
} from 'react'
type Props<T> = Partial<T> & Attributes
const renderChildrenWithProps = <T,>(
  children: ReactNode,
  customizedProps:
    | Props<T>
    | undefined
    | ((child: ReactElement, index: number) => Props<T>)
) =>
  Children.map(children, (child, index) => {
    // Checking isValidElement is the safe way and avoids a
    // typescript error too.
    if (isValidElement(child)) {
      return cloneElement(
        child,
        customizedProps instanceof Function
          ? customizedProps(child, index)
          : customizedProps
      )
    }
    return child
  })

export default renderChildrenWithProps
