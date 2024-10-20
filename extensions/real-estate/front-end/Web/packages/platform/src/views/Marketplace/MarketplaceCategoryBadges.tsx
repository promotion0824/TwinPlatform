import { Badge } from '@willowinc/ui'
import styled from 'styled-components'
import { MarketplaceApp } from './types'

const CategoryBadge = styled(Badge)({
  flexShrink: 0,
})

const CategoryBadges = styled.div(({ theme }) => ({
  display: 'flex',
  flexWrap: 'wrap',
  gap: theme.spacing.s4,
}))

const MappedBadge = styled.div(({ theme }) => ({
  display: 'flex',
  flexWrap: 'wrap',
  gap: theme.spacing.s4,
  color: theme.color.neutral.fg.subtle,
  ...theme.font.body.md.regular,
}))

export default function MarketplaceCategoryBadges({
  app,
}: {
  app: MarketplaceApp
}) {
  const isPoweredByMapped =
    app.developer?.name === 'mapped' &&
    app.supportedApplicationKinds?.includes('marketing')

  return isPoweredByMapped ? (
    <MappedBadge>Powered by Mapped</MappedBadge>
  ) : (
    <CategoryBadges>
      {[...app.categoryNames].sort().map((categoryName) => (
        <CategoryBadge
          color="gray"
          key={categoryName}
          size="sm"
          variant="muted"
        >
          {categoryName}
        </CategoryBadge>
      ))}
    </CategoryBadges>
  )
}
