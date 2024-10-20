// based on https://stackoverflow.com/a/4819886
function isTouchDevice() {
  return (
    'ontouchstart' in window ||
    navigator.maxTouchPoints > 0 ||
    // @ts-expect-error // no types for msMaxTouchPoints
    navigator.msMaxTouchPoints > 0
  )
}

export default isTouchDevice
