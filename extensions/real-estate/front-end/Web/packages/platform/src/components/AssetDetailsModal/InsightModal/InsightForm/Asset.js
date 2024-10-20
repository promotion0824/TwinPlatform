import styled from 'styled-components'
import { Fieldset, Flex, Input, Link } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import routes from '../../../../routes'

export default function Asset({ insight }) {
  const { t } = useTranslation()
  const { asset, siteId, twinId } = insight

  if (asset == null) {
    return null
  }

  return (
    <Fieldset icon="assets" legend={t('headers.assets')}>
      <Flex horizontal>
        <Flex flex={2}>
          {/*
            display asset name as a clickable link which will redirect user to
            twin explorer page focusing on that asset when siteId and twinId
            of the insight are both defined.
            reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/75906
          */}
          {siteId != null && twinId != null ? (
            <>
              <StyledAssetName>{t('labels.assetName')}</StyledAssetName>
              <StyledLink
                to={{
                  pathname: routes.portfolio_twins_view__siteId__twinId(
                    siteId,
                    twinId
                  ),
                  state: {
                    from: location.pathname,
                  },
                }}
              >
                {asset.name}
              </StyledLink>
            </>
          ) : (
            <Input label={t('labels.assetName')} value={asset.name} readOnly />
          )}
        </Flex>
        <Flex flex={1} />
      </Flex>
    </Fieldset>
  )
}

const StyledAssetName = styled.div({
  fontSize: '11px',
  paddingBottom: '8px',
})

const StyledLink = styled(Link)({
  marginTop: 'auto',
  paddingLeft: '9px',
  height: '30px',
  lineHeight: '30px',
  backgroundColor: '#1C1C1C',
  textDecoration: 'underline',

  '&:hover': {
    textDecoration: 'underline',
  },
})
