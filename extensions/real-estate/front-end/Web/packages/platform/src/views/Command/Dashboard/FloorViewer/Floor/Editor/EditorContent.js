import { useEffect } from 'react'
import cx from 'classnames'
import { useAnalytics, Flex } from '@willow/ui'
import { useFloor } from '../FloorContext'
import { useEditor } from './EditorContext'
import useInitializeEditor from './useInitializeEditor'
import useZoomEditor from './useZoomEditor'
import CreateMode from './CreateMode/CreateMode'
import EditMode from './EditMode/EditMode'
import ViewMode from './ViewMode/ViewMode'
import HandlePanEditor from './HandlePanEditor'
import HandleZoomEditor from './HandleZoomEditor'
import Range from './Range/Range'
import styles from './EditorContent.css'

export default function EditorContent() {
  const analytics = useAnalytics()
  const editor = useEditor()
  const floor = useFloor()

  useInitializeEditor()
  const zoomEditor = useZoomEditor()

  const cxClassName = cx(styles.editorContent, {
    [styles.view]: floor.mode === 'view',
    [styles.create]: floor.mode === 'create',
  })

  useEffect(() => {
    analytics.page('Dashboard Floor 2D')
  }, [])

  return (
    <>
      <Flex fill="content hidden" overflow="hidden" className={cxClassName}>
        <div
          ref={editor.contentRef}
          className={styles.content}
          onContextMenu={(e) => e.preventDefault()}
        >
          {floor.modules2D.length > 0 && (
            <svg
              ref={editor.svgRef}
              width={floor.width}
              height={floor.height}
              viewBox={`0 0 ${floor.width} ${floor.height}`}
              className={styles.svg}
              style={{
                transform: `matrix(${editor.zoom / 100}, 0, 0, ${
                  editor.zoom / 100
                }, ${editor.x}, ${editor.y})`,
              }}
            >
              {floor.modules2D.map((image) => (
                <image
                  key={image.id}
                  href={image.url}
                  height="100%"
                  width="100%"
                  className={cx(styles.image, {
                    [styles.visible]: image.isVisible,
                  })}
                />
              ))}
              {floor.mode === 'view' && <ViewMode />}
              {floor.mode === 'create' && <CreateMode />}
              {floor.mode === 'edit' && <EditMode />}
            </svg>
          )}
          <div ref={editor.tooltipsRef} className={styles.tooltips} />
        </div>
        <Flex align="center" className={styles.rangeContainer}>
          <Range
            value={editor.zoom}
            values={editor.zoomLevels}
            className={styles.range}
            onChange={zoomEditor.zoomAtCenter}
          />
        </Flex>
      </Flex>
      <HandlePanEditor />
      <HandleZoomEditor />
    </>
  )
}
