import { useConfig } from 'providers'

export default function FeatureToggle(props) {
  const { feature, children } = props

  const config = useConfig()

  return config.hasFeatureToggle(feature) ? children : null
}
