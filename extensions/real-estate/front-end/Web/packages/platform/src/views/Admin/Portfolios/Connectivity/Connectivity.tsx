import { BackButton, Flex, Text, DocumentTitle } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import LayoutHeaderPanel from '../../../Layout/Layout/LayoutHeaderPanel'
import ConnectivityProvider, {
  useConnectivity,
} from './providers/ConnectivityProvider'
import ConnectivityFilters from './ConnectivityFilters'
import ConnectivityMetric from './ConnectivityMetric'
import ConnectivityTable from './ConnectivityTable'

export default function Connectivity() {
  return (
    <ConnectivityProvider>
      <ConnectivityContent />
    </ConnectivityProvider>
  )
}

function ConnectivityContent() {
  const { t } = useTranslation()
  const {
    portfolioName,
    renderMetricObject,
    connectivityTableData,
    connectivityTableState,
    selectedTab,
    setSelectedTab,
    filters,
    setFilters,
    hasFiltersChanged,
    clearFilters,
  } = useConnectivity()
  return (
    <>
      <DocumentTitle
        scopes={[portfolioName, t('headers.connectivity'), t('headers.admin')]}
      />

      <LayoutHeaderPanel fill="header">
        <Flex horizontal fill="content">
          <BackButton to="/admin" />
          <Flex horizontal size="large">
            <StyledFlex horizontal align="middle">
              <PaddingRightText type="h2" color="text">
                {portfolioName}
              </PaddingRightText>
            </StyledFlex>
            <PaddingFlex horizontal align="middle">
              <Text type="h2" color="text">
                {t('plainText.viewConnectivity')}
              </Text>
            </PaddingFlex>
          </Flex>
        </Flex>
      </LayoutHeaderPanel>

      <MarginTopFlex horizontal fill="content" size="small">
        <ConnectivityFilters
          filters={filters}
          setFilters={setFilters}
          hasFiltersChanged={hasFiltersChanged}
          clearFilters={clearFilters}
        />
        <NoOverflowFlex>
          <ConnectivityMetric renderMetricObject={renderMetricObject} />
          <ConnectivityTable
            connectivityTableData={connectivityTableData}
            connectivityTableState={connectivityTableState}
            selectedTab={selectedTab}
            setSelectedTab={setSelectedTab}
          />
        </NoOverflowFlex>
      </MarginTopFlex>
    </>
  )
}
const StyledFlex = styled(Flex)({
  'border-right': '1px solid #383838',
  padding: ' var(--padding-large)',
})

const PaddingFlex = styled(Flex)({
  padding: '0px 33px 0px 33px',
  'border-right': '1px solid #383838',
  'margin-left': 'unset !important',
})

const MarginTopFlex = styled(Flex)({
  marginTop: '4px',
})

/**
 * Prevent programtical scroll by using clip instead of hidden.
 * The !important keyword is used to override default Flexbox styling
 * Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/72300
 */
const NoOverflowFlex = styled(Flex)({
  overflow: 'clip !important',
})

const PaddingRightText = styled(Text)({ paddingRight: '71px' })
