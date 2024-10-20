import { useEffect, useState } from 'react'
import { useLocation } from 'react-router'
import { useConfig, useSnackbar, useUser } from '@willow/ui'
import { useGetMe } from '@willow/common'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'

export default function Initialize({ children }) {
  const meQuery = useGetMe()
  const config = useConfig()
  const location = useLocation()
  const snackbar = useSnackbar()
  const user = useUser()

  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    async function initialize({ nextUser, meQueryError }) {
      const currentConfig = await config.loadConfig()
      await user.loadUser({ currentConfig, nextUser, meQueryError })
      setIsLoading(false)
    }

    if (meQuery.isError) {
      initialize({ meQueryError: meQuery.error })
    } else if (meQuery.isSuccess && meQuery.data) {
      initialize({ nextUser: meQuery.data })
    }
  }, [meQuery.data, meQuery.isError, meQuery.isSuccess])

  useEffect(() => {
    snackbar.clear()
  }, [location.pathname])

  if (isLoading) {
    return (
      <Flex position="absolute" align="center middle">
        <Icon icon="progress" size="large" />
      </Flex>
    )
  }

  return children ?? null
}
