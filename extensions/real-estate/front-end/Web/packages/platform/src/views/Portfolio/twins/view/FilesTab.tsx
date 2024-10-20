import { useEffect } from 'react'
import { getUrl } from '@willow/ui'

import useTwinAnalytics from '../useTwinAnalytics'
import FileListItem, { ContainmentWrapper } from '../shared/ui/FileListItem'
import List from '../shared/ui/List'

type TwinFile = {
  id: string
  fileName: string
  size?: number
}

type Twin = any

export default function FilesTab({
  twin,
  files,
}: {
  twin: Twin
  files: TwinFile[]
}) {
  const twinAnalytics = useTwinAnalytics()
  useEffect(() => {
    twinAnalytics.trackTwinFilesViewed({ twin })
  }, [])

  return (
    <ContainmentWrapper>
      <List data-testid="files-table">
        {files.map((file) => (
          <FileListItem
            key={file.id}
            fileId={file.id}
            fileName={file.fileName}
            siteId={twin.siteID}
            downloadUrl={getUrl(
              `/api/sites/${twin.siteID}/assets/${twin.uniqueID}/files/${file.id}`
            )}
            disableLink
          />
        ))}
      </List>
    </ContainmentWrapper>
  )
}
