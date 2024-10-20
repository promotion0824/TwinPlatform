import React from 'react'

// https://medium.com/pixel-and-ink/testing-loadable-components-with-jest-97bfeaa6da0b
export function lazy(load) {
  let Component

  const loadPromise = load().then((val) => {
    Component = val.default
  })

  const Loadable = (props) => {
    if (!Component) {
      throw new Error(
        `Bundle split module not loaded yet, ensure you beforeAll(() => MyLazyComponent.load()) in your test, import statement: ${load.toString()}`
      )
    }

    return <Component {...props} />
  }

  Loadable.load = () => loadPromise
  return Loadable
}
