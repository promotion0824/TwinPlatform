import tw from 'twin.macro'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import { useTranslation } from 'react-i18next'

/**
 * Intended to be displayed at the end of some kind of lazy-loaded list of items.
 *
 * - If `isLoading` is true, displays a loading spinner.
 * - If `isError` is true, displays an error message and a retry button.
 * - Calls `onRetry` when the retry button is clicked.
 */
export default function Tail({ isLoading, isError, onRetry }) {
  const { t } = useTranslation()
  return (
    <>
      {isLoading && (
        <>
          {/*
            We load data from the API in pages, so we might have already loaded
            some items but also display an indicator that we are loading more.
          */}
          <div tw="justify-center">
            <Flex align="center middle" padding="large">
              <Icon icon="progress" />
            </Flex>
          </div>
        </>
      )}
      {isError && (
        <div tw="justify-center">
          <Flex align="center middle" padding="large" tw="flex-row">
            <Icon icon="error" tw="ml-2" />
            {t('plainText.longErrorMessage')}
            <Button color="grey" tw="ml-4" onClick={onRetry}>
              {t('plainText.retry')}
            </Button>
          </Flex>
        </div>
      )}
    </>
  )
}
