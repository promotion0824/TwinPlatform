import { useState } from 'react'
import { Flex, TabsHeader } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import WorkgroupsContent from './WorkgroupsContent'
import WorkgroupsModal from './WorkgroupsModal/WorkgroupsModal'
import { WorkgroupsContext } from './WorkgroupsContext'

export default function Workgroups({ sites }) {
  const { t } = useTranslation()
  const [selectedWorkgroup, setSelectedWorkgroup] = useState()
  const [selectedSite, setSelectedSite] = useState()
  const [search, setSearch] = useState('')

  function handleAddWorkgroupClick() {
    setSelectedWorkgroup({
      id: null,
      siteId: null,
      name: '',
      users: [],
    })
  }

  const context = {
    sites,
    selectedWorkgroup,
    selectedSite,
    search,

    setSelectedWorkgroup,
    setSelectedSite,
    setSearch,
  }

  return (
    <WorkgroupsContext.Provider value={context}>
      <TabsHeader>
        <Flex align="right middle" padding="0 medium">
          <Button
            onClick={handleAddWorkgroupClick}
            prefix={<Icon icon="add" />}
          >
            {t('plainText.addWorkGroup')}
          </Button>
        </Flex>
      </TabsHeader>
      <WorkgroupsContent />
      {selectedWorkgroup != null && (
        <WorkgroupsModal
          workgroup={selectedWorkgroup}
          sites={sites}
          onClose={() => setSelectedWorkgroup()}
        />
      )}
    </WorkgroupsContext.Provider>
  )
}
