import { useCallback, useRef } from 'react'

/**
 * Call the `onView` callback when the element to which this ref is attached becomes visible.
 * Adapted from https://github.com/WebDevSimplified/React-Infinite-Scrolling/blob/master/src/App.js
 *
 * The `getObservableElements` can be used in the case where the element to
 * which the ref is attached is a container which doesn't take up any space
 * itself (for example it may have `display: contents`). Such an element will
 * never receive an intersection event itself, but you can tell `useIntersectionObserverRef` to
 * listen for intersections with its children instead by passing
 * `getObservableElements: (node) => node.children`.
 *
 * Example usage:

    function MyComponent({ items }) {
      const myRef = useIntersectionObserverRef({
        onView: () => {
          // ask for new pages or something
        }
      });

      return <>
        {items.map((item, i) => {
          // Call the onView handler when the last item in the list is visible
          return <div ref={i === items.length - 1 ? myRef : null}>
            {item}
          </div>
       })}
      </>;
    }
  */
export default function useIntersectionObserverRef(
  { onView, getObservableElements = (node) => [node] },
  dependencies
) {
  const observer = useRef()

  return useCallback((node) => {
    if (observer.current) {
      observer.current.disconnect()
    }
    observer.current = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          onView()
        }
      },
      {
        threshold: 0,
      }
    )
    if (node) {
      Array.from(getObservableElements(node)).forEach((cell) => {
        observer.current.observe(cell)
      })
    }
  }, dependencies)
}
