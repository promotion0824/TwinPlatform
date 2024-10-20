import _ from 'lodash'
import { styled } from 'twin.macro'
import pluralize from 'pluralize'
import { useTranslation } from 'react-i18next'
import { Icon, Progress, TwinChip } from '@willow/ui'
import { Group, Popover, UnstyledButton } from '@willowinc/ui'

import useTwinAnalytics from '../useTwinAnalytics'
import IconRight from './icon.chevron.right.svg'
import IconOpen from './icon.open.twin.svg'
import IconLocate from './icon.location.svg'
import { useTwinView } from './TwinViewContext'
import { TwinLink } from '../shared'

const Container = styled.div({
  textAlign: 'left',
  paddingBottom: '1rem',
})

const Heading = styled.h4({
  fontSize: 'var(--font-small)',
  fontWeight: 'var(--font-weight-500)',
  margin: '0.5rem 0',
})

const Chips = styled.div({
  display: 'flex',
  flexWrap: 'wrap',
  gap: '0.5rem',
})

const More = styled(UnstyledButton)({
  display: 'flex',
  alignItems: 'center',
  paddingLeft: '0.5rem',

  background: 'var(--theme-color-neutral-bg-accent-default)',

  color: 'var(--lighter)',
  fontFamily: 'var(--font)',
  fontSize: 'var(--font-extra-tiny)',
  fontWeight: 'var(--font-weight-500)',

  textTransform: 'uppercase',

  '&:hover': {
    color: 'var(--light)',
  },

  '& svg path': {
    color: 'inherit',
    fill: 'currentColor',
  },
})

/**
 * Render the list of related twins, grouped by model of interest. If any of
 * the related twins are not covered by a model of interest, display them in an
 * "Other" group at the end. If none of the twins are covered by a model of
 * interest, the "Other" group is instead labelled "Related Twins".
 */
export default function TwinRelationships({ modelsOfInterest, relationships }) {
  const { t } = useTranslation()

  if (!relationships) {
    return (
      <Container>
        <Progress />
      </Container>
    )
  }

  const relationshipsByModel = modelsOfInterest.map((modelOfInterest) =>
    relationships.filter(
      (r) => r.target.modelOfInterest?.modelId === modelOfInterest.modelId
    )
  )
  const coveredRelationships = new Set(relationshipsByModel.flat())
  const otherRelationships = relationships.filter(
    (r) => !coveredRelationships.has(r)
  )

  return (
    <Container>
      {_.zip(modelsOfInterest, relationshipsByModel).map(
        ([modelOfInterest, relationshipsOfType]) => {
          if (!relationshipsOfType.length) {
            return null
          }

          return (
            <RelationshipsOfType
              key={modelOfInterest.id}
              heading={pluralize(modelOfInterest.name)}
              relationshipsOfType={relationshipsOfType}
              modelOfInterest={modelOfInterest}
            />
          )
        }
      )}
      {otherRelationships.length > 0 && (
        <RelationshipsOfType
          heading={
            coveredRelationships.size > 0
              ? _.capitalize(t('plainText.other', 'other'))
              : t('headers.relatedTwins')
          }
          relationshipsOfType={otherRelationships}
          modelOfInterest={{}}
        />
      )}
    </Container>
  )
}

const RelationshipsOfType = ({
  relationshipsOfType,
  heading,
  modelOfInterest,
}) => {
  const { t } = useTranslation()

  const { setTab } = useTwinView()

  // If we have more relationships than this, we show a button to open the
  // relationships tab which will show all of them.
  const maxRelationshipsDisplayed = 8

  const relationshipsDisplayed = relationshipsOfType.slice(
    0,
    maxRelationshipsDisplayed
  )

  function handleDisplayMoreClick() {
    setTab('relationships')
  }

  return (
    <>
      <Heading>{heading}</Heading>
      <Chips>
        {relationshipsDisplayed.map((relationship) => (
          <Relationship
            key={relationship.id}
            modelOfInterest={modelOfInterest}
            target={relationship.target}
            id={relationship.id}
            name={relationship.name}
          />
        ))}
        {relationshipsOfType.length > maxRelationshipsDisplayed ? (
          <More onClick={handleDisplayMoreClick}>
            {t('interpolation.Xmore', {
              count: relationshipsOfType.length - maxRelationshipsDisplayed,
            })}
            <IconRight />
          </More>
        ) : null}
      </Chips>
    </>
  )
}

const StyledLink = styled(TwinLink)({
  display: 'flex',
  alignItems: 'center',
  color: '#959595',
  fontSize: '9px',
  fontWeight: '500',
  textTransform: 'uppercase',
  textDecoration: 'none',

  '&:hover': {
    color: '#D9D9D9',
    textDecoration: 'none',
  },
  maxWidth: '100%',
})

const Relationship = ({ modelOfInterest, target, id, name }) => {
  const { locateTwin } = useTwinView()
  const { t } = useTranslation()
  const analytics = useTwinAnalytics()

  // Show "Locate" button except when related twin has a virtual (autogenerated)
  // relationship or sensor (hostedBy) relationship
  const showLocateTwin = !/^AUTOGENERATED/i.test(id) && name !== 'hostedBy'

  return (
    <Popover position="right">
      <Popover.Target>
        <UnstyledButton>
          <TwinChip
            variant="instance"
            modelOfInterest={modelOfInterest}
            text={target.name || `(${t('plainText.unnamedTwin')})`}
            icon={<Icon size="tiny" icon="more" />}
            highlightOnHover
          />
        </UnstyledButton>
      </Popover.Target>
      <Popover.Dropdown>
        <Group pl="s8">
          <StyledLink
            twin={target}
            onClick={() =>
              analytics.trackRelatedTwinAction({
                option: 'open',
                twin: target.name,
              })
            }
          >
            {t('plainText.open')} <IconOpen />
          </StyledLink>
          {showLocateTwin && (
            <StyledLink
              as={UnstyledButton}
              onClick={() => {
                locateTwin(target.id)
                analytics.trackRelatedTwinAction({
                  option: 'locate',
                  twin: target.name,
                })
              }}
            >
              {t('plainText.locate')} <IconLocate />
            </StyledLink>
          )}
        </Group>
      </Popover.Dropdown>
    </Popover>
  )
}
