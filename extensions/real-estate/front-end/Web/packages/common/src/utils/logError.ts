/**
 * Throw error in development mode, otherwise log error in console.
 * Recommend to use when no error is expected but just in case of page crash.
 */
const throwErrorInDevelopmentMode = (error: string) => {
  if (process.env.NODE_ENV === 'development') {
    throw new Error(error)
  } else {
    console.error(error)
  }
}

export default throwErrorInDevelopmentMode
