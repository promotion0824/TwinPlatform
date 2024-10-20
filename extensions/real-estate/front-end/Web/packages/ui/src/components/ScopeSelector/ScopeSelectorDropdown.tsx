import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import { Icon, IconButton, TextInput } from '@willowinc/ui'

import type { LocationNode } from './ScopeSelector'
import ScopeSelectorTree from './ScopeSelectorTree'
import { flattenTree } from './scopeSelectorUtils'

const Heading = styled.div(({ theme }) => ({
  ...theme.font.heading.group,
  padding: theme.spacing.s8,
  textTransform: 'uppercase',
}))

const TreeContainer = styled.div(({ theme }) => ({
  padding: theme.spacing.s8,
}))

export default function ScopeSelectorDropdown({
  locations,
  onSelect,
  portfolios,
  selectedLocation,
  ...restProps
}: {
  locations: LocationNode[]
  onSelect: (selectedLocation: LocationNode) => void
  portfolios: LocationNode[]
  selectedLocation: LocationNode
}) {
  const { t } = useTranslation()

  // auto focus on the search input on dropdown open
  const inputRef = useRef<HTMLInputElement>(null)
  useEffect(() => {
    if (inputRef.current) {
      inputRef.current.focus()
    }
  }, [])

  const [searchTerm, setSearchTerm] = useState('')
  const flattenedLocations = flattenTree(locations)
    .map((location) => {
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      const { children: _, ...rest } = location
      return { ...rest }
    })
    .sort((a, b) => a.twin.name.localeCompare(b.twin.name))

  return (
    <TreeContainer {...restProps}>
      <TextInput
        onChange={(event) => setSearchTerm(event.currentTarget.value)}
        placeholder={t('labels.search')}
        prefix={<Icon icon="search" />}
        suffix={
          searchTerm.length > 0 && (
            <IconButton background="transparent" kind="secondary">
              <Icon icon="close" onClick={() => setSearchTerm('')} />
            </IconButton>
          )
        }
        value={searchTerm}
        ref={inputRef}
      />

      {!searchTerm.length ? (
        <>
          <Heading>{t('headers.portfolios')}</Heading>
          <ScopeSelectorTree
            data={portfolios}
            onSelect={onSelect}
            selectedLocation={selectedLocation}
            variant="portfolios"
          />

          <Heading>{t('headers.locations')}</Heading>
          <ScopeSelectorTree
            data={locations}
            onSelect={onSelect}
            selectedLocation={selectedLocation}
          />
        </>
      ) : (
        <>
          <Heading>{t('headers.locations')}</Heading>
          <ScopeSelectorTree
            data={flattenedLocations}
            onSelect={onSelect}
            searchTerm={searchTerm}
            selectedLocation={selectedLocation}
            variant="search"
          />
        </>
      )}
    </TreeContainer>
  )
}
