import { useEffect, useState, useRef } from 'react'
import tw from 'twin.macro'
import { DataPanel } from '@willow/ui'

export const Sigma = tw.div`
  h-full
`

export const StyledIframe = tw.iframe`
  h-full
  w-full
  px-1
`
export default function SigmaReport({ embedURL, startDate, endDate, siteIds }) {
  const [workbookLoaded, setWorkbookLoaded] = useState({})
  const isLoading = Object.keys(workbookLoaded).length === 0

  const sigmaRef = useRef()

  function handleLoadingScreen() {
    window.addEventListener('message', (event) => {
      if (
        event.source === sigmaRef.current?.contentWindow &&
        event.origin === 'https://app.sigmacomputing.com'
      ) {
        setWorkbookLoaded(event.data)
      }
    })
  }

  useEffect(() => {
    handleLoadingScreen()
    setWorkbookLoaded({})
    return () => {
      window.removeEventListener('message', handleLoadingScreen)
    }
  }, [setWorkbookLoaded, startDate, endDate, siteIds])

  return (
    <DataPanel isLoading={isLoading}>
      <Sigma
        style={{
          display: isLoading ? 'none' : 'block',
        }}
      >
        <StyledIframe
          title="Sigma Report"
          ref={sigmaRef}
          src={embedURL}
          frameBorder="0"
        />
      </Sigma>
    </DataPanel>
  )
}
