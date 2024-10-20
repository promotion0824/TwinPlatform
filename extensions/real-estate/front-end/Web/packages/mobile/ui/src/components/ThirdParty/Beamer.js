import { useEffect } from 'react'
import Script from 'components/Script/Script'
import { useConfig } from '../../providers/config/ConfigContext'

export default function Beamer() {
  const { beamerApiKey } = useConfig()

  useEffect(() => {
    window.beamer_config = {
      product_id: beamerApiKey,
      selector: '.beamerTrigger',
      button: false,
      right: -22,
      top: -5,
    }
  }, [])

  return (
    <Script
      id="beamer-snippet"
      src="https://app.getbeamer.com/js/beamer-embed.js"
    />
  )
}
