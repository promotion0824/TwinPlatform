import { Link, Text } from '@willow/ui'
import styled, { CSSProp } from 'styled-components'
import { useTranslation } from 'react-i18next'
import { TextWithTooltip } from '@willow/common/insights/component'

/**
 * Text is clickable and displayed as a link when siteId and twinId are defined,
 * otherwise displayed as plain text
 */
const AssetLink = ({
  path,
  className,
  assetName,
  siteId,
  twinId,
  css,
}: {
  path: string
  className?: string
  assetName?: string
  siteId?: string
  twinId?: string
  css?: CSSProp
}) => {
  const { t } = useTranslation()

  return siteId && twinId ? (
    <StyledLink
      to={path}
      className={className}
      css={css}
      onClick={(e) => e.stopPropagation()}
    >
      <TextWithTooltip
        text={assetName ?? t('plainText.noNameFound')}
        tooltipWidth="200px"
        isTitleCase={false}
      />
    </StyledLink>
  ) : (
    <FormattedText>{assetName ?? '--'}</FormattedText>
  )
}

export default AssetLink

const StyledLink = styled(Link)({
  textTransform: 'capitalize',
  font: '400 12px/30px Poppins',
  textAlign: 'center',
  whiteSpace: 'nowrap',
  textOverflow: 'ellipsis',
  overflow: 'hidden',
  textDecoration: 'underline',
  height: '100%',
})

const FormattedText = styled(Text)({
  textTransform: 'capitalize',
  whiteSpace: 'nowrap',
  textAlign: 'center',
  lineHeight: '30px !important',
  height: '30px',
})
