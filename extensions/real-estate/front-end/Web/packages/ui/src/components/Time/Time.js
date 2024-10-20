import { useState } from 'react'
import cx from 'classnames'
import { useEffectOnceMounted } from '@willow/common'
import { useDateTime } from '@willow/ui'
import { useLanguage } from '../../providers/LanguageProvider/LanguageContext'
import styles from './Time.css'

export default function Time({
  value,
  timezone,
  format = 'dateTime',
  className,
}) {
  const dateTime = useDateTime()
  const { language } = useLanguage()

  const [formattedValue, setFormattedValue] = useState(() =>
    dateTime(value, timezone).format(format, timezone, language)
  )

  useEffectOnceMounted(() => {
    setFormattedValue(
      dateTime(value, timezone).format(format, timezone, language)
    )
  }, [value, timezone, format, language])

  const cxClassName = cx(styles.time, className)

  return <span className={cxClassName}>{formattedValue}</span>
}
