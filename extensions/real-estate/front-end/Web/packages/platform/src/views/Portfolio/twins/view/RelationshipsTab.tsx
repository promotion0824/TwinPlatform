/* eslint-disable no-use-before-define */
import { Ontology } from '@willow/common/twins/view/models'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  getContainmentHelper,
  Input,
  Option,
  Progress,
  Select,
  Text,
} from '@willow/ui'
import { Icon } from '@willowinc/ui'
import _ from 'lodash'
import { Children, ReactNode, useEffect, useState } from 'react'
import { Trans, useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import useOntology from '../../../../hooks/useOntologyInPlatform'
import {
  formatRelationshipName,
  TwinLinkListItem,
  TwinModelChip,
} from '../shared'
import List from '../shared/ui/List'
import useTwinAnalytics from '../useTwinAnalytics'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

type Relationship = {
  id: string
  name: string
  target: {
    id: string
    siteId: string
    modelId: string
    modelOfInterest: string
    name: string
  }
}

const FilterContainer = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s8,
  marginBottom: theme.spacing.s12,

  [getContainerQuery('width <= 380px')]: {
    flexDirection: 'column',
    width: '100%',
  },
}))

const SearchIconContainer = styled.div(({ theme }) => ({
  alignItems: 'center',
  color: theme.color.neutral.fg.muted,
  display: 'flex',
  padding: `0 ${theme.spacing.s4}`,
}))

export default function RelationshipsTab({
  twin,
  relationships,
  modelsOfInterest,
}: {
  twin: any
  relationships: Relationship[]
  modelsOfInterest: ModelOfInterest[]
}) {
  const { t } = useTranslation()
  const ontologyQuery = useOntology()
  const analytics = useTwinAnalytics()

  // If the user has typed into the name filter, we only show related twins
  // whose names contain the entered text (case insensitive).
  const [nameFilter, setNameFilter] = useState('')

  // If the user has selected an option other than "all relationship types"
  // (null), we only show related twins of the specified type.
  const [relationshipTypeFilter, setRelationshipTypeFilter] = useState<
    string | null
  >(null)

  useEffect(() => {
    if (relationships) {
      analytics.trackRelatedTwinsViewed({ twin, count: relationships.length })
    }
  }, [analytics, relationships, twin])

  const ontology = ontologyQuery.data

  if (ontology == null || !relationships) {
    return <Progress />
  }

  let displayedRelationships = relationships.filter(
    (r) =>
      (r.target.name ?? '').toUpperCase().includes(nameFilter.toUpperCase()) &&
      (relationshipTypeFilter == null || r.name === relationshipTypeFilter)
  )

  // The backend sometimes returns the same relationship twice. Since we use
  // the relationship id as a React key, this can result in duplicate React
  // keys and cause the filtering to behave incorrectly, so we make sure to
  // remove duplicates.
  displayedRelationships = _.uniqBy(displayedRelationships, (r) => r.id)

  function handleNameFilterChange(name: string) {
    if (nameFilter === '') {
      // To avoid worrying about debouncing issues, we only send a single event
      // on the first character typed (and again if they happen to clear and
      // type again).
      analytics.trackRelatedTwinsNameFilterTyped()
    }
    setNameFilter(name)
  }

  function handleRelationshipTypeFilterChange(type: null | string) {
    analytics.trackRelatedTwinsRelationshipTypeChanged({ type })
    setRelationshipTypeFilter(type)
  }

  return (
    <ContainmentWrapper>
      <TabContent>
        <div tw="relative">
          <div data-testid="tab-relatedTwins-list" tw="flex flex-col">
            <ListHeading data-testid="tab-relatedTwins-header">
              <div tw="flex flex-wrap">
                <div tw="flex flex-1 items-center margin-bottom[12px] margin-right[8px] max-width[100%]">
                  <div tw="whitespace-nowrap overflow-ellipsis overflow-hidden">
                    <Trans
                      i18nKey="interpolation.relatedTwinsCount"
                      defaults="<0>{{ num }}</0> twins related to <1>{{ twinName }}</1>"
                      count={relationships.length}
                      values={{
                        num: relationships.length,
                        twinName: twin.name || t('plainText.unnamedTwin'),
                      }}
                      components={[
                        // If you update these components, make sure to maintain the
                        // `fixSlashes` behaviour (see below in this file).
                        <Highlight />,
                        <Highlight />,
                      ]}
                      shouldUnescape
                    />
                  </div>
                </div>
                <FilterContainer>
                  <NameFilter
                    type="text"
                    value={nameFilter}
                    placeholder={t('plainText.filterByName')}
                    preservePlaceholder
                    onChange={handleNameFilterChange}
                    icon={
                      <SearchIconContainer>
                        <Icon icon="search" />
                      </SearchIconContainer>
                    }
                  />
                  <RelationshipTypeFilter
                    relationships={relationships}
                    value={relationshipTypeFilter}
                    onChange={handleRelationshipTypeFilterChange}
                  />
                </FilterContainer>
              </div>
            </ListHeading>
            <ListBody>
              {displayedRelationships.map((relationship) => (
                <RelationshipListItem
                  key={relationship.id}
                  relationship={relationship}
                  ontology={ontologyQuery.data}
                  modelsOfInterest={modelsOfInterest}
                />
              ))}
            </ListBody>
          </div>
        </div>
      </TabContent>
    </ContainmentWrapper>
  )
}

function RelationshipTypeFilter({
  value,
  onChange,
  relationships,
}: {
  value: null | string
  onChange: (val: null | string) => void
  relationships: Relationship[]
}) {
  const { t } = useTranslation()

  // Display "all relationship types" (null) first, then the distinct
  // relationship types from the items in the list.
  const options = [null, ..._.uniq(relationships.map((r) => r.name)).sort()]

  function formatOption(option: null | string) {
    if (option == null) {
      return t('plainText.allRelationshipTypes')
    } else {
      return formatRelationshipName(option)
    }
  }

  return (
    <RelationshipTypeSelect value={value} header={formatOption}>
      {options.map((option) => (
        <Option key={option} onClick={() => onChange(option)}>
          {formatOption(option)}
        </Option>
      ))}
    </RelationshipTypeSelect>
  )
}

function RelationshipListItem({
  relationship,
  ontology,
  modelsOfInterest,
}: {
  relationship: Relationship
  ontology: Ontology
  modelsOfInterest: ModelOfInterest[]
}) {
  const { t } = useTranslation()
  const analytics = useTwinAnalytics()

  return (
    <li key={relationship.id}>
      <TwinLinkListItem
        twin={relationship.target}
        onClick={() =>
          analytics.trackRelatedTwinAction({
            option: 'open',
            twin: relationship.target.name,
          })
        }
      >
        <div tw="mb-2 flex">
          <TitleBlock tw="overflow-hidden">
            <Text
              type="h2"
              weight="medium"
              title={relationship.target.name || t('plainText.unnamedTwin')}
            >
              {relationship.target.name || t('plainText.unnamedTwin')}
            </Text>
          </TitleBlock>
          <TitleBlock>
            <RelationshipType>
              ({formatRelationshipName(relationship.name)})
            </RelationshipType>
          </TitleBlock>
        </div>
        <TwinModelChip
          twin={relationship.target}
          ontology={ontology}
          modelsOfInterest={modelsOfInterest}
        />
      </TwinLinkListItem>
    </li>
  )
}

const NameFilter = styled(Input)({
  fontSize: 11,
  width: '100%',
  height: 24,
  display: 'flex',
  alignItems: 'center',
  '&&& input': {
    paddingLeft: 0,
    fontSize: 11,
  },
})

const RelationshipTypeSelect = styled(Select)({
  width: '100%',
  fontSize: 11,
  height: 24,
})

/**
 * There is a bug (https://github.com/i18next/react-i18next/issues/1487) in react-i18next where
 * the Trans component does not unescape forward slashes correctly. So while we wait for that
 * bug to be fixed, we unescape them ourselves.
 */
function fixSlashes(Component: React.FC<{ children?: React.ReactNode }>) {
  return ({ children }: { children?: ReactNode }) => (
    <Component>
      {Children.map(children, (c) =>
        typeof c === 'string' ? c.replaceAll('&#x2F;', '/') : c
      )}
    </Component>
  )
}

const Highlight = fixSlashes(
  styled.span({
    fontWeight: 'bold',
    color: '#d9d9d9',
  })
)

const TitleBlock = styled.div({
  display: 'inline-block',
  verticalAlign: 'middle',
})

const RelationshipType = styled.span({
  font: '10px Poppins',
  fontWeight: 500,
  color: '#959595',
  textTransform: 'uppercase',
  marginLeft: 8,
  whiteSpace: 'nowrap',
})

const TabContent = styled.div({
  position: 'relative',
  height: '100%',
})

const ListHeading = styled.div(({ theme }) => ({
  padding: '16px 16px 0 16px',
  fontWeight: 'var(--font-weight-500)',
  fontSize: 'var(--font-tiny)',
  position: 'sticky',
  top: 0,
  backgroundColor: theme.color.neutral.bg.panel.default,
}))

const ListBody = styled(List)({
  padding: '0 16px',
})
