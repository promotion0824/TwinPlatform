import { Progress, Message, useIntersectionObserverRef } from '@willow/ui'

import { useSearchResults as InjectedSearchResults } from '../../state/SearchResults'
import NoTwinsFound from './NoTwinsFound'
import InjectedResultsList from './ResultsList'
import InjectedResultsTable from './ResultsTable'

const Results = ({
  disableLinks = false,
  ResultsList = InjectedResultsList,
  ResultsListButton = ({ twin }) => <></>,
  ResultsTable = InjectedResultsTable,
  useSearchResults = InjectedSearchResults,
}) => {
  const { t, display, twins, isLoading, isError, fetchNextPage } =
    useSearchResults()

  const endOfPageRef = useIntersectionObserverRef(
    {
      onView: fetchNextPage,
    },
    [twins]
  )

  return (
    <>
      {isError ? (
        <Message icon="error">{t('plainText.errorOccurred')}</Message>
      ) : isLoading ? (
        <Progress />
      ) : !twins.length ? (
        <NoTwinsFound t={t} />
      ) : display === 'list' ? (
        <ResultsList
          disableLinks={disableLinks}
          endOfPageRef={endOfPageRef}
          ResultsButton={ResultsListButton}
        />
      ) : (
        <ResultsTable endOfPageRef={endOfPageRef} />
      )}
    </>
  )
}

export default Results
