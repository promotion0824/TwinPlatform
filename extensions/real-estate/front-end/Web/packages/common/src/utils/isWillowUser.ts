const isWillowUser = (email?: string): boolean =>
  /@willowinc.com$/.test(email ?? '')

export default isWillowUser
