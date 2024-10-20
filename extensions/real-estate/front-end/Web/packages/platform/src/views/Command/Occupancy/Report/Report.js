import { useParams } from 'react-router'
import { Fetch } from '@willow/ui'
import ReportContent from './ReportContent'

export default function ReportComponent() {
  const params = useParams()

  return (
    <Fetch url={`/api/sites/${params.siteId}/dashboard`}>
      {(response) => <ReportContent selectedReport={response.widgets[0]} />}
    </Fetch>
  )
}
