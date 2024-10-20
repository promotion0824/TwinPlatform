import {
  Fetch,
  Flex,
  Tab,
  TabBackButton,
  Tabs,
  DocumentTitle,
} from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { useSite } from 'providers'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import FloorModal from './FloorModal'
import FloorsTable from './FloorsTable'

export default function Floors() {
  const params = useParams()
  const { t } = useTranslation()
  const { name } = useSite()

  const [selectedFloor, setSelectedFloor] = useState()

  function handleAddFloorClick() {
    setSelectedFloor({
      code: '',
      name: '',
    })
  }

  return (
    <>
      <DocumentTitle
        scopes={[
          t('headers.floors'),
          name,
          t('plainText.buildings'),
          t('headers.admin'),
        ]}
      />

      <Flex fill="header" padding="small 0 0 0">
        <Tabs $borderWidth="1px 0 0 0">
          <TabBackButton />
          <Tab header={t('headers.floors')}>
            <Fetch
              name="floors"
              url={`/api/sites/${params.siteId}/floors`}
              params={{
                hasBaseModule: false,
              }}
            >
              {(floors) => (
                <FloorsTable
                  floors={floors}
                  selectedFloor={selectedFloor}
                  setSelectedFloor={setSelectedFloor}
                />
              )}
            </Fetch>
          </Tab>
          <Flex align="right middle" padding="0 medium">
            <Button onClick={handleAddFloorClick} prefix={<Icon icon="add" />}>
              {t('plainText.addFloor')}
            </Button>
          </Flex>
        </Tabs>
      </Flex>
      {selectedFloor != null && (
        <FloorModal floor={selectedFloor} onClose={() => setSelectedFloor()} />
      )}
    </>
  )
}
