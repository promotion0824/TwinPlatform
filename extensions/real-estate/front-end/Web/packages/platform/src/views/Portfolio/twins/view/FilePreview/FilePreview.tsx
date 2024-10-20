import { getExtension, NotFound, Progress } from '@willow/ui'
import { Button } from '@willowinc/ui'
import { debounce } from 'lodash'
import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Document, Page, pdfjs } from 'react-pdf'
import { styled } from 'twin.macro'
import FilePreviewControls, { DEFAULT_PDF_SCALE } from './FilePreviewControls'

pdfjs.GlobalWorkerOptions.workerSrc = '/public/pdf.worker.min.js'

const supportedFileTypes = ['jpg', 'pdf', 'png', 'txt']

const PreviewContainer = styled.div<{ scale: number }>(({ scale }) => ({
  height: '100%',

  // Centralise the pdf when the scale is smaller than default,
  // which means the document will be smaller than container in both
  //  width and height. This style will cause problem when scaled up,
  // so wrapped in condition.
  ...(scale < DEFAULT_PDF_SCALE
    ? {
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
      }
    : {}),

  img: {
    maxWidth: '100%',
  },

  object: {
    width: '100%',
  },
}))

const StyledNotFound = styled(NotFound)({
  height: 'auto',
  margin: '0 !important',
})

const UnsupportedContainer = styled.div({
  alignItems: 'center',
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
  justifyContent: 'center',
})

export default function FilePreview({
  fileName,
  fileUrl,
  ...rest
}: {
  fileName: string
  fileUrl: string
}) {
  const { t } = useTranslation()

  const [pdfLoaded, setPdfLoaded] = useState<boolean | 'error'>(false)
  const [pdfPageCount, setPdfPageCount] = useState(0)
  const [pdfCurrentPage, setPdfCurrentPage] = useState<number>(0)
  const [pdfScale, setPdfScale] = useState<number>(DEFAULT_PDF_SCALE)
  const containerRef = useRef<HTMLDivElement>(null)
  const [containerWidth, setContainerWidth] = useState<number>(0)
  const [containerHeight, setContainerHeight] = useState<number>(0)

  useEffect(() => {
    if (containerRef.current) {
      // initialise width of the component
      setContainerWidth(containerRef.current.offsetWidth)
      setContainerHeight(containerRef.current.offsetHeight)
    }
  }, [])

  useEffect(
    () => {
      const pdfContainerElement = containerRef?.current

      if (pdfContainerElement) {
        const resizeObserver = new ResizeObserver(
          debounce(() => (entries) => {
            entries.forEach((entry) => {
              setContainerWidth(entry.contentRect.width)
              setContainerHeight(entry.contentRect.height)
            })
          })
        )

        resizeObserver.observe(pdfContainerElement)

        // Clean up
        return () => {
          resizeObserver.unobserve(pdfContainerElement)
        }
      }

      return undefined
    },
    // Can also use [] here as the container element will never change.
    // But for safety, we use the element as a dependency.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [containerRef.current]
  )

  function handleDocumentLoadSuccess({ numPages }: { numPages: number }) {
    setPdfLoaded(true)
    setPdfPageCount(numPages)
    setPdfCurrentPage(1)
  }

  const fileType = getExtension(fileName).toLowerCase().slice(1)

  return !supportedFileTypes.includes(fileType) ? (
    <UnsupportedContainer {...rest}>
      <StyledNotFound>{t('plainText.noPreviewAvailable')}</StyledNotFound>
      <Button
        kind="secondary"
        onClick={() => window.open(fileUrl, '_blank')}
        size="large"
      >
        {t('labels.download')}
      </Button>
    </UnsupportedContainer>
  ) : (
    <PreviewContainer ref={containerRef} scale={pdfScale} {...rest}>
      {fileType === 'pdf' ? (
        <>
          <Document
            file={fileUrl}
            loading=""
            onLoadSuccess={handleDocumentLoadSuccess}
            onLoadError={() => setPdfLoaded('error')}
          >
            <Page
              pageNumber={pdfCurrentPage}
              renderAnnotationLayer={false}
              renderTextLayer={false}
              scale={pdfScale}
              width={containerWidth}
              css={{
                // override the default white background of the page
                backgroundColor: 'transparent !important',
                // only make the PDF contained when it's initial scale
                ...(pdfScale === DEFAULT_PDF_SCALE
                  ? {
                      canvas: {
                        width: `${containerWidth}px !important`,
                        height: `${containerHeight}px !important`,
                        objectFit: 'contain',
                      },
                    }
                  : {}),
              }}
            />
          </Document>

          {pdfLoaded !== 'error' &&
            (pdfLoaded ? (
              <FilePreviewControls
                currentPage={pdfCurrentPage}
                onPageChange={setPdfCurrentPage}
                pageCount={pdfPageCount}
                onScaleChange={setPdfScale}
                currentScale={pdfScale}
              />
            ) : (
              <Progress />
            ))}
        </>
      ) : fileType === 'jpg' || fileType === 'png' ? (
        <img alt={fileName || 'TODO:'} src={fileUrl} />
      ) : fileType === 'txt' ? (
        <object
          aria-label={fileName || 'TODO:'}
          data={fileUrl}
          type="text/plain"
        />
      ) : undefined}
    </PreviewContainer>
  )
}
