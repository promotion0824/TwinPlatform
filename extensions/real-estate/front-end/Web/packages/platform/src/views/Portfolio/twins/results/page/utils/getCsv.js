import { api } from '@willow/ui'

function getAttachmentFilename(contentDisposition) {
  // Adapted from https://stackoverflow.com/a/67994693
  const utf8FilenameRegex = /filename\*=UTF-8''([\w%\-\.]+)(?:; ?|$)/i
  const asciiFilenameRegex = /filename=(["']?)(.*?[^\\])\1(?:; ?|$)/i

  let filename
  if (utf8FilenameRegex.test(contentDisposition)) {
    filename = decodeURIComponent(utf8FilenameRegex.exec(contentDisposition)[1])
  } else {
    const matches = asciiFilenameRegex.exec(contentDisposition)
    if (matches != null && matches[2]) {
      // eslint-disable-next-line prefer-destructuring
      filename = matches[2]
    }
  }
  return filename
}

export default async function getCsv({
  fileType,
  modelId,
  queryId,
  siteIds,
  scopeId,
  term,
  twins,
  useCognitiveSearch,
}) {
  if (!useCognitiveSearch && queryId == null) {
    throw new Error('We really, really need a queryId')
  }

  const response = useCognitiveSearch
    ? await api.post('/twins/cognitiveSearch/export', {
        export: true,
        fileType,
        modelId,
        siteIds,
        scopeId,
        term,
        twinIds: twins?.map((twin) => twin.id),
      })
    : await api.post('/twins/export', {
        queryId,
        twins: twins?.map((t) => ({ siteId: t.siteId, twinId: t.id })),
      })

  const blob = new Blob([response.data], { type: 'text/csv' })
  const link = document.createElement('a')
  const url = window.URL.createObjectURL(blob)
  link.href = url

  let filename
  if (response.headers['content-disposition'] != null) {
    filename = getAttachmentFilename(response.headers['content-disposition'])
  } else {
    filename = 'Twins-export.csv'
  }
  link.download = filename
  link.click()
  window.URL.revokeObjectURL(url)
}
