import { useEffect, useState } from 'react'
import { useLocation } from 'react-router'
import { useConfig, useSnackbar, useUser } from 'providers'
import Loader from 'components/Loader/Loader'
import styles from './Initialize.css'

export default function Initialize(props) {
  const { children } = props

  const location = useLocation()
  const snackbar = useSnackbar()
  const config = useConfig()
  const user = useUser()
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    async function initialize() {
      const currentConfig = await config.loadConfig()
      await user.loadUser(currentConfig)
      setIsLoading(false)
    }

    initialize()
  }, [])

  useEffect(() => {
    snackbar.clear()
  }, [location.pathname])

  if (isLoading) {
    return <Loader size="extraLarge" className={styles.center} />
  }

  return children ?? null
}
