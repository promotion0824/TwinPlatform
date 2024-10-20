import { titleCase } from '@willow/common'
import { getUrl } from '@willow/ui'
import { TabAndPanel, Tabs } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import FilePreview from './FilePreview'

type LastUpdatedTime = {
  lastUpdatedTime: string
}

type DocumentTwin = {
  description: string
  etag: string
  id: string
  metadata: {
    description: LastUpdatedTime
    modelId: string
    name: LastUpdatedTime
    siteId: LastUpdatedTime
    uniqueId: LastUpdatedTime
    url: LastUpdatedTime
  }
  name: string
  siteID: string
  uniqueID: string
  url: string
}

export default function useFilePreviewTab({
  twin,
}: {
  twin?: DocumentTwin
}): TabAndPanel | undefined {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  if (!twin) {
    return undefined
  }

  const fileUrl = getUrl(
    `/api/sites/${twin.siteID}/twins/${twin.id}/download/?inline=true`
  )

  return [
    <Tabs.Tab value="filePreview">
      {titleCase({ text: t('headers.preview'), language })}
    </Tabs.Tab>,
    <Tabs.Panel value="filePreview">
      <FilePreview fileName={twin.name} fileUrl={fileUrl} />
    </Tabs.Panel>,
  ]
}
