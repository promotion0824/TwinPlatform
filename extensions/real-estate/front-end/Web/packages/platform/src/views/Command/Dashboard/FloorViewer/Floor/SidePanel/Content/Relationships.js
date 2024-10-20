import { useParams } from 'react-router'
import { Fetch, Table, Head, Body, Row, Cell } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function Relationships({ assetId }) {
  const params = useParams()
  const { t } = useTranslation()

  return (
    <Fetch
      url={`/api/pilot/sites/${params.siteId}/assets/${assetId}/relationships`}
    >
      {(response) => (
        <Table items={response.relationships}>
          {(relationships) => (
            <>
              <Head>
                <Row>
                  <Cell>{t('plainText.relationship')}</Cell>
                  <Cell>{t('plainText.target')}</Cell>
                </Row>
              </Head>
              <Body>
                {relationships.map((relationship) => (
                  <Row key={relationship.relationship.id}>
                    <Cell>{relationship.relationship.name}</Cell>
                    <Cell type="fill">{relationship.target.name}</Cell>
                  </Row>
                ))}
              </Body>
            </>
          )}
        </Table>
      )}
    </Fetch>
  )
}
