import { styled } from 'twin.macro'
import { titleCase } from '@willow/common'
import { useTranslation } from 'react-i18next'
import { Site } from '@willow/common/site/site/types'
import { Flex, Select, Option, SiteFavoriteButton, ALL_SITES } from '@willow/ui'
import { AllSites } from '../../providers/sites/SiteContext'

export default function SiteSelect({
  sites,
  value,
  onChange,
  isAllSiteIncluded = true,
}: {
  sites: Site[]
  value: Site
  onChange: (site: Site | AllSites) => void
  isAllSiteIncluded: boolean
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const allSites = titleCase({ language, text: t('headers.allSites') })

  return (
    <SelectWithFixedWidth
      value={value}
      header={(site: Site) => (site?.id != null ? site.name : allSites)}
      unselectable
      data-cy="siteSelect-dropdown"
      className="siteSelect"
    >
      {isAllSiteIncluded && (
        <OptionWithPadding
          $isSelected={value?.id == null}
          value={allSites}
          onClick={() => onChange({ id: null, name: ALL_SITES })}
        >
          {allSites}
        </OptionWithPadding>
      )}
      {sites.map((site) => (
        <OptionContainer
          data-cy="site-option-dropdown"
          key={site.id}
          horizontal
          fill="header hidden"
          align="middle"
        >
          <OptionWithPadding
            $isSelected={value === site}
            value={site}
            onClick={() => onChange(site)}
          >
            {site.name}
          </OptionWithPadding>
          <FavoriteButtonWithFixedSize
            className="siteFavoriteButton"
            siteId={site.id}
            onClick={() => {}} // temp fix for type error
          />
        </OptionContainer>
      ))}
    </SelectWithFixedWidth>
  )
}

const OptionWithPadding = styled(Option)<{ $isSelected?: boolean }>(
  ({ $isSelected }) => ({
    color: $isSelected ? '#D9D9D9' : '#A4A5A6',
    padding: 'var(--padding)',
  })
)

const SelectWithFixedWidth = styled(Select)({
  width: '317px',
})

const OptionContainer = styled(Flex)({
  minHeight: '38px',
})

const FavoriteButtonWithFixedSize = styled(SiteFavoriteButton)({
  height: '38px',
  width: '38px',
})
