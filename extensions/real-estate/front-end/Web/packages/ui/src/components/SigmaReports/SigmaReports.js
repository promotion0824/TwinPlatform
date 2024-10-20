import { useEffect, useState } from 'react'
import tw from 'twin.macro'
import { useApi } from '@willow/ui'
import DataPanel from 'components/DataPanel/DataPanel'

export const Sigma = tw.div`
  h-full
  border-t-1 border-gray-550 border-solid
  `

export const StyledIframe = tw.iframe`
  h-full
  w-full
  `
export default function SigmaReports({ siteId, reportId, isSigma }) {
  const [report, setReport] = useState(null)
  const [workbookLoaded, setWorkbookLoaded] = useState({})
  const iFrame = document.getElementById('sigma-iframe')

  const api = useApi()
  const url = `/api/sigma/sites/${siteId}/embedurl?reportId=${reportId}`
  const isLoading = Object.keys(workbookLoaded).length === 0

  function handleLoadingScreen() {
    if (isSigma) {
      window.addEventListener('message', (event) => {
        if (
          event.source === iFrame?.contentWindow &&
          event.origin === 'https://app.sigmacomputing.com'
        ) {
          setWorkbookLoaded(event.data)
        }
      })
    }
  }
  handleLoadingScreen()

  async function fetchReports() {
    const res = await api.get(url)
    setReport(await res.url)
  }

  useEffect(() => {
    fetchReports()
    setWorkbookLoaded({})
  }, [setWorkbookLoaded, reportId, siteId])

  return (
    <DataPanel isLoading={isLoading}>
      <Sigma
        style={{
          display: isLoading ? 'none' : 'block',
        }}
      >
        <StyledIframe
          title="Sigma Report"
          id="sigma-iframe"
          src={report}
          frameBorder="0"
        />
      </Sigma>
    </DataPanel>
  )
}
