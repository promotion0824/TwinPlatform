import ResizeObserver from 'resize-observer-polyfill'

// set the observer globally before importing app
window.ResizeObserver = window.ResizeObserver || ResizeObserver
