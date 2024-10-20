import { Group } from '@willowinc/ui'
import { compact } from 'lodash'
import { css } from 'styled-components'
import { styled } from 'twin.macro'
import TwinTypeSvg from './TwinTypeSvg'

// variants: "ontology" | "instance"
const ONTOLOGY = 'ontology'

const TwinTypeIcon = styled(TwinTypeSvg)(({ theme, variant }) => ({
  height: 24,
  width: 24,
  backgroundColor:
    variant === ONTOLOGY
      ? theme.color.neutral.border.default
      : theme.color.neutral.bg.panel.default,
  flexShrink: 0,
}))

const Container = styled.div(
  ({ theme, variant, $isSelected, $hasEndPadding, $highlightOnHover }) => ({
    display: 'inline-flex',
    alignItems: 'center',
    backgroundColor:
      variant === ONTOLOGY
        ? theme.color.neutral.bg.panel.default
        : theme.color.neutral.border.default,

    border: $isSelected
      ? `solid 1px ${theme.color.neutral.fg.subtle}`
      : 'solid 1px #383838',
    color: $isSelected ? 'var(--light)' : theme.color.neutral.fg.muted,
    '&:hover': {
      color: $highlightOnHover ? 'var(--light)' : undefined,
    },

    borderRadius: 2,
    paddingRight: $hasEndPadding ? theme.spacing.s4 : undefined,
    verticalAlign: 'top',
    maxWidth: '100%',
  })
)

const BaseText = styled.div(({ theme }) => ({
  borderLeft: `solid thin ${theme.color.neutral.border.default}`,
  whiteSpace: 'nowrap',
  height: 24,
  lineHeight: '16px',
  padding: '4px 8px',
  fontFamily: 'Poppins',
  fontWeight: 500,
  textOverflow: 'ellipsis',
  overflow: 'hidden',

  '&:last-of-type': {
    flexShrink: 2,
  },
}))

const OntologyText = styled(BaseText)({
  fontSize: 9,
  textTransform: 'uppercase',
})
const InstanceText = styled(BaseText)(({ theme }) => ({
  fontSize: 10,
  color: theme.color.neutral.fg.default,
}))

const Gap = styled(BaseText)({
  padding: 0.5,
})

const Count = styled(BaseText)({
  borderLeft: 'none',
  fontSize: 9,
  textTransform: 'none',
  fontWeight: 'bold',
  color: '#d9d9d9',
})

const CountInner = styled.span({
  backgroundColor: '#2b2b2b',
  padding: '2px 4px',
  borderRadius: 2,
})

const TwinChip = ({
  variant = ONTOLOGY, // ? ontology | instance
  modelOfInterest = undefined,
  text = undefined, // ? string | string[]
  gappedText = undefined, // ? string
  icon = undefined, // ? ReactElement

  /**
   * If provided, the count is displayed in the rightmost slot in the chip, and
   * prefixed with an "x".
   */
  count = undefined,
  $isSelected = false,
  className = undefined,
  title = undefined,
  onClick = undefined,
  $highlightOnHover = false,

  'data-testid': dataTestId = undefined,

  additionalInfo = undefined,
}) => {
  const Text = variant === ONTOLOGY ? OntologyText : InstanceText
  const textArray = Array.isArray(text) ? text : [text]
  const compactedAdditionalInfo = compact(additionalInfo)

  return (
    <Container
      className={className}
      variant={variant}
      $hasEndPadding={!!icon}
      $isSelected={$isSelected}
      title={title}
      onClick={onClick}
      $highlightOnHover={$highlightOnHover}
      data-testid={dataTestId}
    >
      <TwinTypeIcon variant={variant} modelOfInterest={modelOfInterest} />

      {text ? (
        textArray.map((piece, index) => (
          <Text key={index} title={piece}>
            {piece}
          </Text>
        ))
      ) : modelOfInterest != null ? (
        <Text title={modelOfInterest.name}>{modelOfInterest.name}</Text>
      ) : null}

      {gappedText && (
        <>
          <Gap />
          <Text title={gappedText}>{gappedText}</Text>
        </>
      )}

      {icon}

      {count != null && (
        <Count>
          <CountInner>x{count}</CountInner>
        </Count>
      )}

      {compactedAdditionalInfo && compactedAdditionalInfo.length > 0 && (
        <Group
          pl="s6"
          pr="s6"
          gap="s4"
          bg="neutral.bg.accent.default"
          h="24" // same as how other parts's height get defined
          css={css(({ theme }) => ({
            borderTopRightRadius: theme.radius.r2,
            borderBottomRightRadius: theme.radius.r2,
            flexShrink: 0, // so it won't shrink to multiple rows when twin name is long
          }))}
        >
          {compactedAdditionalInfo}
        </Group>
      )}
    </Container>
  )
}

export default TwinChip
