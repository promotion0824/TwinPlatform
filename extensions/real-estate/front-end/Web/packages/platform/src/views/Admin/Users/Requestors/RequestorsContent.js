import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Flex, TabsHeader } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import Requestors from './Requestors/Requestors'

export default function RequestorsContent({ requestors, sites }) {
  const { t } = useTranslation()
  const [selectedRequestor, setSelectedRequestor] = useState()

  function handleAddRequestorClick() {
    setSelectedRequestor({})
  }

  return (
    <>
      <TabsHeader>
        <Flex align="right middle" padding="0 medium">
          <Button
            onClick={handleAddRequestorClick}
            prefix={<Icon icon="add" />}
          >
            {t('plainText.addRequestor')}
          </Button>
        </Flex>
      </TabsHeader>
      <Requestors
        requestors={requestors}
        sites={sites}
        selectedRequestor={selectedRequestor}
        setSelectedRequestor={setSelectedRequestor}
      />
    </>
  )
}
