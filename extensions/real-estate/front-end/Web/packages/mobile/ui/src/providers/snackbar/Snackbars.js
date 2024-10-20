import { useState } from 'react'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Component from 'components/Component/Component'
import Message from 'components/Message/Message'
import Portal from 'components/Portal/Portal'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import Transition from 'components/Transition/Transition'
import { useSnackbar } from './SnackbarContext'
import styles from './Snackbars.css'

export default function Snackbars() {
  const snackbar = useSnackbar()
  const [timerId, setTimeId] = useState(null)

  function handleSnackbarMount(bar, transition) {
    const id = window.setTimeout(() => {
      if (bar.timeout != null) {
        transition.close()
      }
    }, bar.timeout)
    setTimeId(id)

    return () => {
      window.clearTimeout(timerId)
    }
  }

  const handleMouseEnter = () => {
    if (timerId) {
      window.clearTimeout(timerId)
      setTimeId(null)
    }
  }

  return (
    <Portal>
      <div className={styles.snackbars} onMouseEnter={handleMouseEnter}>
        {snackbar.snackbars.map((bar) => (
          <Transition
            key={bar.snackbarId}
            className={styles.snackbar}
            onClose={() => snackbar.hide(bar.snackbarId)}
          >
            {(transition) => (
              <>
                <Component
                  onMount={() => handleSnackbarMount(bar, transition)}
                />
                <Spacing horizontal align="center" className={styles.container}>
                  <Spacing
                    className={cx(styles.content, 'ignore-onclickoutside')}
                  >
                    {bar.messages.map((message, i) => (
                      <Message
                        key={i} // eslint-disable-line
                        horizontal
                        padding="large"
                        align="left middle"
                        type="content"
                        whiteSpace="normal"
                        icon={message.icon}
                        className={styles.message}
                      >
                        <Spacing size="tiny">
                          {message.header != null && (
                            <Text
                              color="light"
                              size="large"
                              whiteSpace="normal"
                            >
                              {message.header}
                            </Text>
                          )}
                          {message.description != null && (
                            <Text whiteSpace="normal">
                              {message.description}
                            </Text>
                          )}
                        </Spacing>
                      </Message>
                    ))}
                  </Spacing>
                  <Button
                    icon="close"
                    className={cx(styles.close, 'ignore-onclickoutside')}
                    onClick={() => transition.close()}
                  />
                </Spacing>
              </>
            )}
          </Transition>
        ))}
      </div>
    </Portal>
  )
}
