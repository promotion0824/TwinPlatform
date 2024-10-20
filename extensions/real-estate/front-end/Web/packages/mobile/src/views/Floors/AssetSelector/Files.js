import { useParams } from 'react-router'
import { native } from 'utils'
import {
  getApiGlobalPrefix,
  Button,
  Fetch,
  Icon,
  NotFound,
  Spacing,
} from '@willow/mobile-ui'
import { useLayout } from 'providers'

export default function Files() {
  const params = useParams()
  const { setShowBackButton } = useLayout()
  const isNative = native.functionExists('showPdf')

  setShowBackButton(true)

  const showPdfInNative = (url) => {
    native.showPdf(url)
  }

  return (
    <>
      <Fetch url={`/api/sites/${params.siteId}/assets/${params.assetId}/files`}>
        {(files) => (
          <Spacing type="content">
            {files.length > 0 && (
              <Spacing size="medium">
                {files.map((fileItem) => (
                  <Spacing key={fileItem.id} horizontal>
                    {isNative ? (
                      <Button
                        onClick={() =>
                          showPdfInNative(
                            `${getApiGlobalPrefix()}/api/sites/${
                              params.siteId
                            }/assets/${params.assetId}/files/${fileItem.id}`
                          )
                        }
                        data-segment="PDF File Opened"
                      >
                        <Spacing padding="medium">
                          <Icon icon="file" />
                        </Spacing>
                        {fileItem.fileName}
                      </Button>
                    ) : (
                      <Button
                        href={`${getApiGlobalPrefix()}/api/sites/${
                          params.siteId
                        }/assets/${params.assetId}/files/${
                          fileItem.id
                        }?inline=true`}
                        target="_blank"
                        data-segment="PDF File Opened"
                      >
                        <Spacing padding="medium">
                          <Icon icon="file" />
                        </Spacing>
                        {fileItem.fileName}
                      </Button>
                    )}
                  </Spacing>
                ))}
              </Spacing>
            )}
            {files.length === 0 && (
              <Spacing>
                <NotFound>No files found</NotFound>
              </Spacing>
            )}
          </Spacing>
        )}
      </Fetch>
    </>
  )
}
