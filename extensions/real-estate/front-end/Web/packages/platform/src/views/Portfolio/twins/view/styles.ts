export const fontStyle = {
  color: 'var(--light)',
  fontSize: 11,
  // This corresponds to the desired "medium" font weight from the XD design.
  fontWeight: 500,
  input: {
    // Create additional rules with "input" in the selector so we don't get overridden
    // by the user agent stylesheet.
    fontSize: 11,
    fontWeight: 500,
  },
}

export const subheadingStyle = {
  ...fontStyle,
  color: 'var(--light)',
}
