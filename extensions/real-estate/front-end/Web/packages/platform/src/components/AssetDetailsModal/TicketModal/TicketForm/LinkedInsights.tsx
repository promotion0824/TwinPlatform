import { Link, useForm } from '@willow/ui'
import Fieldset from '@willow/ui/components/Fieldset/Fieldset'
import { Badge, Icon } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import _ from 'lodash'
import { TFunction } from 'i18next'
import { TextWithTooltip } from '@willow/common/insights/component'
import { useState } from 'react'
import {
  DependentDiagnostic,
  InsightDiagnosticResponse,
} from '../../../../services/Tickets/TicketsService'

/**
 * This type is only relevant when we are associating diagnostics insights to specific ticket
 */
interface Diagnostic {
  id: DependentDiagnostic['id']
  name: DependentDiagnostic['name']
  originalIndex?: number
}

export default function LinkedInsights({
  insightDiagnostic,
  siteId,
  isReadOnly = false,
}: {
  insightDiagnostic: InsightDiagnosticResponse
  siteId: string
  isReadOnly?: boolean
}) {
  const form = useForm()
  const { t } = useTranslation()
  const { id, name, ruleName } = insightDiagnostic
  const [removedDiagnostics, setRemovedDiagnostics] = useState<Diagnostic[]>([])

  /**
   * This function adds a diagnostic to form data if its not present and remove it from removedDiagnostics once user removes it.
   * Once its removed, it is added back to form data at the original position in the list
   */
  const handleDiagnosticClick = (diagnostic: Diagnostic) => {
    const addedDiagnostics = form.data.diagnostics ?? []
    const index = addedDiagnostics.findIndex(
      (existingItem) => existingItem.id === diagnostic.id
    )
    if (index !== -1) {
      form.setData((prevData) => ({
        ...prevData,
        diagnostics: _.xorBy(addedDiagnostics, [diagnostic], 'id'),
      }))
      setRemovedDiagnostics([...removedDiagnostics, diagnostic])
    } else {
      setRemovedDiagnostics(_.xorBy(removedDiagnostics, [diagnostic], 'id'))
      form.setData((prevData) => ({
        ...prevData,
        diagnostics: _.sortBy(
          [...addedDiagnostics, diagnostic],
          'originalIndex'
        ),
      }))
    }
  }

  return (
    <StyledFieldset
      icon="reset"
      legend={_.startCase(t('plainText.linkedInsights'))}
    >
      <InsightSection t={t} name={ruleName || name} id={id} siteId={siteId} />
      {((form.data.diagnostics ?? []).length > 0 ||
        removedDiagnostics.length > 0) && (
        <>
          <StyledLabel tw="mb-2">
            {_.startCase(t('plainText.diagnosticDependencies'))}
          </StyledLabel>
          <StyledSection>
            {(form.data.diagnostics ?? []).map((addedDiagnostic) => (
              <InsightSection
                t={t}
                key={addedDiagnostic.id}
                name={addedDiagnostic.name}
                id={addedDiagnostic.id}
                siteId={siteId}
                width="431px"
                isClose={!isReadOnly}
                onIconClick={() => handleDiagnosticClick(addedDiagnostic)}
              />
            ))}
            {removedDiagnostics.map((removedDiagnostic) => (
              <InsightSection
                t={t}
                key={removedDiagnostic.id}
                name={removedDiagnostic.name}
                id={removedDiagnostic.id}
                siteId={siteId}
                width="431px"
                isClose
                isRemoved
                onIconClick={() => handleDiagnosticClick(removedDiagnostic)}
              />
            ))}
          </StyledSection>
        </>
      )}
    </StyledFieldset>
  )
}

function InsightSection({
  t,
  id,
  siteId,
  name,
  isClose,
  isRemoved = false,
  width = '478px',
  onIconClick,
}: {
  t: TFunction
  id: string
  siteId: string
  name: string
  isRemoved?: boolean
  isClose?: boolean
  width?: string
  onIconClick?: (diagnostic: Diagnostic) => void
}) {
  return (
    <Container>
      {width === '431px' && (
        <Badge
          variant="subtle"
          color="red"
          size="md"
          css={css(({ theme }) => ({
            marginRight: theme.spacing.s8,
          }))}
        >
          {_.capitalize(t('plainText.fail'))}
        </Badge>
      )}
      <StyledDiv $width={width} $isRemoved={isRemoved}>
        <NewPageLink>
          <Icon icon="open_in_new" size={20} />
          <StyledLink
            href={`/sites/${siteId}/insights/${id}`}
            target="_blank"
            onClick={(e) => e.stopPropagation()}
          >
            <TextWithTooltip
              text={name}
              tooltipWidth="200px"
              isTitleCase={false}
              tw="max-w-[350px]"
            />
          </StyledLink>
        </NewPageLink>
        {isClose && (
          <Icon
            css={css({
              marginLeft: 'auto',
              cursor: 'pointer',
            })}
            icon={isRemoved ? 'add' : 'close'}
            size={20}
            onClick={() => onIconClick?.({ id, name })}
          />
        )}
      </StyledDiv>
    </Container>
  )
}

const StyledFieldset = styled(Fieldset)({
  '> div:first-child': {
    overflow: 'hidden',
  },
})

const StyledSection = styled.section({
  maxHeight: '184px',
  overflow: 'auto',

  '&&&': {
    marginTop: '0',
  },
})

const StyledLabel = styled.span(({ theme }) => ({
  marginBottom: theme.spacing.s16,

  '&&&': {
    marginTop: '0',
  },
}))

const StyledDiv = styled.div<{ $width: string; $isRemoved: boolean }>(
  ({ theme, $width, $isRemoved }) => ({
    display: 'flex',
    padding: `${theme.spacing.s4} ${theme.spacing.s8}`,
    alignItems: 'center',
    gap: theme.spacing.s8,
    borderRadius: '2px',
    border: $isRemoved
      ? 'none'
      : `1px solid ${theme.color.neutral.border.default}`,
    background: $isRemoved ? 'none' : theme.color.neutral.bg.accent.default,
    width: $width,
  })
)

const Container = styled.div(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  width: '100%',
  marginBottom: theme.spacing.s16,

  '& > div:last-child': {
    marginBottom: '0px',
  },
}))

const NewPageLink = styled.div({
  display: 'flex',
  alignItems: 'center',
})

const StyledLink = styled(Link)(({ theme }) => ({
  textDecoration: 'underline',
  color: theme.color.neutral.fg.highlight,
  marginLeft: theme.spacing.s8,
  '&&&': {
    cursor: 'pointer',
  },
}))
