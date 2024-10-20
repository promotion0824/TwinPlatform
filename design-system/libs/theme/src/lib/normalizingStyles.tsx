const CommonStyles = {
  button: {
    padding: 0,
    outline: 'none',
    border: 'none',
  },

  fieldset: {
    border: 'none',
    margin: 0,
    padding: 0,
  },

  legend: {
    padding: 0,
  },

  input: {
    outline: 'none',
  },
} as const

/**
 * We did not apply those styles to globalStyles because they were applied to
 * the top-level of an app that uses our theme and components. However, there
 * are many existing components in the app, which are not ours, that might be
 * impacted by this style override. Therefore, we're only applying these
 * styles at our component level to limit the potential impact. In the future,
 * we may move them to globalStyles once we could evaluate the safety of the
 * impact.
 */
const getElementNormalizingStyle = (elementType: keyof typeof CommonStyles) => {
  return CommonStyles[elementType]
}

export default getElementNormalizingStyle
