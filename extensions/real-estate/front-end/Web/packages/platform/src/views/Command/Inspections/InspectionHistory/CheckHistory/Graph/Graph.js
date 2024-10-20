import { useMemo, useRef } from 'react'
import { Flex, NotFound } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import GraphContent from './GraphContent/GraphContent'
import XAxis from './XAxis/XAxis'
import styles from './Graph.css'
import { GraphContext } from './GraphContext'
import colors from './colors.json'
import constructGraphLines from './getGraphData'
import getGraph from './utils/getGraph'

export default function Graph({ check, checkRecordsHistory, times }) {
  const contentRef = useRef()
  const { t } = useTranslation()
  const points = checkRecordsHistory?.filter(
    (x) => x.numberValue !== undefined && x.numberValue !== null
  )

  const typeValues = _.compact(_.uniq(_.map(points, 'typeValue')))

  const graphData = useMemo(() => {
    const lines = typeValues.map((typeValue, index) => {
      const safeIndex = index % colors.length // Safely wrapping the index
      return constructGraphLines(
        check,
        points.filter((x) => x.typeValue === typeValue),
        colors[safeIndex]
      )
    })
    const yAxis = typeValues.join('/')
    // Construct the final graph data structure
    return [{ id: check?.id, type: 'line', lines, yAxis }]
  }, [typeValues, points, check])

  if (!points || !points.length) {
    return (
      <NotFound icon="graph">{t('plainText.inspectionCheckHistoy')}</NotFound>
    )
  }

  const graph = getGraph(graphData.flat(), {
    minX: times[0],
    maxX: times[1],
    minY: undefined,
    maxY: undefined,
  })

  const context = {
    contentRef,
    times,
    ...graph,
  }

  return (
    <GraphContext.Provider value={context}>
      <Flex fill="content hidden" overflow="hidden" className={styles.graph}>
        <GraphContent />
        <XAxis />
      </Flex>
    </GraphContext.Provider>
  )
}
