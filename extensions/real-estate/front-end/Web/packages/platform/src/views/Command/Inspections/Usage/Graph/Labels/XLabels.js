import { useDateTime, useLanguage } from '@willow/ui'
import { useGraph } from '../GraphContext'
import styles from './Labels.css'

export default function XLabels() {
  const graph = useGraph()
  const dateTime = useDateTime()
  const { language } = useLanguage()
  const dataCount = graph?.columns?.length

  return (
    <>
      {graph.columns.map((column, i) => (
        <span
          key={i} // eslint-disable-line
          className={styles.xLabel}
          style={{ left: column.left }}
        >
          {dateTime(new Date(column.name)).format(
            /* 
            graph data comes from our backend, user selects "Last quarter", data count is 3, 
            when user selects "Last year", data count is 12; 
            format expected for above cases will be Month + year,
            the rest of the cases will have format to be Date (day, month , year)
            */
            dataCount === 3 || dataCount === 12 ? 'monthAndYear' : 'date',
            null,
            language
          )}
        </span>
      ))}
    </>
  )
}
