import { compact } from 'lodash'
import { Helmet } from 'react-helmet-async'

/**
 * Will add page title as format of `scopes[0] - scopes[1] - Willow`.
 * If no scopes are provided, it will only add `Willow` as title.
 * If you don't want to add `Willow` as suffix, you can pass `withSuffix` as `false`.
 */
const DocumentTitle = ({
  scopes = [],
  withSuffix = true,
}: {
  scopes?: (string | undefined | null | false)[]
  withSuffix?: boolean
}) => (
  <Helmet>
    <title>
      {compact(scopes.concat(withSuffix ? 'Willow' : undefined)).join(' - ')}
    </title>
  </Helmet>
)

export default DocumentTitle
