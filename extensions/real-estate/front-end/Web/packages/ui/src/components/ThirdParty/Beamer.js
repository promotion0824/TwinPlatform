import { useEffect } from 'react'
import { useConfig, useLanguage } from '@willow/ui'
import Script from './Script'

export default function Beamer() {
  const { beamerApiKey } = useConfig()
  const { language } = useLanguage()

  useEffect(() => {
    if (window.beamer_config == null && beamerApiKey != null) {
      window.beamer_config = {
        product_id: beamerApiKey,
        selector: '.beamerTrigger',
        button: false,
        right: -22,
        top: -5,
        language, // set initial language
      }
    }
    window?.Beamer?.update({ language })
  }, [beamerApiKey, language])

  return (
    <Script
      id="beamer-snippet"
      src="https://app.getbeamer.com/js/beamer-embed.js"
    />
  )
}
