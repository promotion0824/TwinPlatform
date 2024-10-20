import { useRef, useState, useEffect, useCallback } from 'react'
import { useLocation } from 'react-router'
import { useAnalytics, useUser } from '@willow/mobile-ui'
import { LayoutContext } from './LayoutContext'
import Header from './Header/Header'

export { default as LayoutHeader } from './LayoutHeader'

export default function LayoutComponent({ sites, children }) {
  const location = useLocation()
  const analytics = useAnalytics()
  const user = useUser()

  const headerRef = useRef()
  const [title, setTitle] = useState()
  const [subTitle, setSubTitle] = useState()
  const [showBackButton, _setShowBackButton] = useState(false)
  const [backUrl, setBackUrl] = useState(null)

  const setShowBackButton = useCallback(
    (nextVisible, newBackUrl = null) => {
      _setShowBackButton(nextVisible)
      setBackUrl(newBackUrl)
    },
    [_setShowBackButton, setBackUrl]
  )

  const match = location.pathname.match(/\/sites\/(.+?)(\/|$)/)
  const site =
    sites.find(
      (prevSite) => prevSite.id === (match?.[1] ?? user.options.siteId)
    ) ?? sites[0]

  const context = {
    headerRef,
    title,
    subTitle,
    showBackButton,
    backUrl,
    sites,
    site,

    selectSite(nextSite) {
      user.saveUserOptions('siteId', nextSite.id)
    },

    getSiteUrl(siteId) {
      return location.pathname.replace(
        /\/sites\/(.+?)(\/|$)/,
        `/sites/${siteId}${match[2]}`
      )
    },

    setShowBackButton,
    setTitle,
    setSubTitle,
  }

  useEffect(() => {
    analytics.initializeSiteContext(context.site)
    // eslint-disable-next-line react/destructuring-assignment
  }, [context.site])

  return (
    <LayoutContext.Provider value={context}>
      <Header />
      {children}
    </LayoutContext.Provider>
  )
}
