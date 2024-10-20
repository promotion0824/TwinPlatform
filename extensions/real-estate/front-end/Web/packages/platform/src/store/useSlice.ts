import useStore from './createStore'

function useSlice<T, U>(sliceName: string, selector?: (state: T) => U): U {
  return useStore((state) =>
    selector ? selector(state[sliceName]) : state[sliceName]
  )
}

export default useSlice
