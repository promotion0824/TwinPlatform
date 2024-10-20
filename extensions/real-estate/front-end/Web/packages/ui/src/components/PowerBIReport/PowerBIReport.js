import { PowerBIEmbed } from 'powerbi-client-react'
import Fetch from 'components/Fetch/Fetch'
import styles from './PowerBIReport.css'

export default function PowerBIReport({ groupId, reportId, embedUrl }) {
  const EMBED = 1

  return (
    <Fetch url={`/api/powerbi/groups/${groupId}/reports/${reportId}/token`}>
      {(report) => (
        <PowerBIEmbed
          embedConfig={{
            id: reportId,
            embedUrl: embedUrl != null ? embedUrl(report) : report.url,
            accessToken: report.token,
            type: 'report',
            tokenType: EMBED,
          }}
          cssClassName={styles.powerBI}
        />
      )}
    </Fetch>
  )
}
