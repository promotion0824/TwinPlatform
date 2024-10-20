import { useRef } from 'react'
import { Button, Flex } from '@willow/ui'
import styles from './Scroll.css'

export default function Scroll({ children }) {
  const contentRef = useRef()

  function handleUpClick() {
    contentRef.current.scrollTo({
      top: contentRef.current.scrollTop - 100,
      left: 0,
      behavior: 'smooth',
    })
  }

  function handleDownClick() {
    contentRef.current.scrollTo({
      top: contentRef.current.scrollTop + 100,
      left: 0,
      behavior: 'smooth',
    })
  }

  return (
    <Flex fill="content">
      <Button icon="chevron" className={styles.up} onClick={handleUpClick} />
      <div ref={contentRef} className={styles.content}>
        {children}
      </div>
      <Button
        icon="chevron"
        className={styles.down}
        onClick={handleDownClick}
      />
    </Flex>
  )
}
