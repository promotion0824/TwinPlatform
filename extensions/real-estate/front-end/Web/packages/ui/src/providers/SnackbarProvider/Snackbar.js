import { useEffect, useRef, useState } from 'react'
import cx from 'classnames'
import { useTimer } from '@willow/ui'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import Panel from 'components/Panel/Panel'
import Text from 'components/Text/Text'
import { useSnackbar } from './SnackbarContext'
import styles from './Snackbar.css'

export default function Snackbar({ snackbar }) {
  const snackbarContext = useSnackbar()
  const timer = useTimer()

  const snackbarRef = useRef()

  const [autoClose, setAutoClose] = useState(true)
  const [style, setStyle] = useState()
  const cxClassName = cx(
    styles.snackbar,
    {
      [styles.iconError]: snackbar.icon === 'error',
      [styles.iconOk]: snackbar.icon === 'ok',
      [styles.isClosing]: snackbar.isClosing,
      [styles.hideAutoClose]: !autoClose,
    },
    'ignore-onclickoutside'
  )

  const { snackbarId } = snackbar

  async function hide() {
    setStyle({
      maxHeight: snackbarRef.current.offsetHeight,
    })

    snackbarContext.hide({ snackbarId })
  }

  async function close() {
    await timer.sleep(200)

    snackbarContext.close({ snackbarId })
  }

  useEffect(() => {
    if (snackbar.isClosing) {
      close()
    }
  }, [snackbar.isClosing])

  useEffect(() => {
    async function start() {
      await timer.setTimeout(5200)
      hide()
    }
    start()
  }, [])

  return (
    <Flex ref={snackbarRef} className={cxClassName} style={style}>
      <Panel
        horizontal
        fill="header"
        className={styles.content}
        onMouseEnter={() => {
          timer.clearTimeout()

          setAutoClose(false)
        }}
      >
        <Flex
          horizontal
          fill="content"
          align="middle"
          size="medium"
          padding="large"
        >
          {snackbar.icon ? <Icon icon={snackbar.icon} size="large" /> : <div />}
          <Flex size="small">
            {snackbar.message != null && (
              <Text type="message" size="large" color="white">
                {snackbar.message}
              </Text>
            )}
            {snackbar.description != null && (
              <Text type="message" size="tiny">
                {snackbar.description}
              </Text>
            )}
          </Flex>
        </Flex>
        <Button icon="cross" className={styles.close} onClick={() => hide()} />
      </Panel>
    </Flex>
  )
}
