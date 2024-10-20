import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import tw, { styled } from 'twin.macro'
import { css } from 'styled-components'
import { Input, TextArea } from '@willow/ui'
import {
  InsightCostImpactPropNames,
  getImpactScore,
  filterMap,
  titleCase,
} from '@willow/common'
import AssetLink from '@willow/common/components/AssetLink'
import {
  PriorityName,
  InsightDetail,
  Container,
} from '@willow/common/insights/component'
import { Insight } from '@willow/common/insights/insights/types'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import useOntology from '@willow/common/twins/hooks/useOntology'
import { getModelDisplayName } from '@willow/common/twins/view/models'
import { useSites } from '../../../../../providers'
import routes from '../../../../../routes'

export default function InsightDetails({
  insight,
  language,
}: {
  insight: Insight
  language: Language
}) {
  const translation = useTranslation()
  const { t } = translation
  const sites = useSites()
  const ontologyQuery = useOntology(insight.siteId)

  const modelId = ontologyQuery.data?.getModelById?.(
    insight.primaryModelId ?? ''
  )

  const priorityValue = getImpactScore({
    impactScores: insight.impactScores,
    scoreName: InsightCostImpactPropNames.priorityScore,
    language,
  })

  return (
    <Container $hidePaddingBottom>
      <InsightDetail
        headerIcon="details"
        headerText={t('plainText.insightDetails')}
      >
        {[
          {
            label: <Label>{t('labels.description')}</Label>,
            value: insight.description,
          },
          {
            label: <Label>{t('labels.recommendation')}</Label>,
            value: insight?.recommendation,
          },
        ].map(
          ({ label, value }) =>
            value && (
              <TextArea
                key={label}
                label={label}
                value={value}
                readOnly
                tw="w-[560px]"
              />
            )
        )}
        <div
          css={css(({ theme }) => ({
            display: 'flex',
            gap: theme.spacing.s16,
          }))}
        >
          {[
            {
              label: t('labels.priority'),
              children: (
                <div tw="absolute top-[52%] left-[8px]">
                  <PriorityName
                    impactScorePriority={Number(priorityValue)}
                    insightPriority={insight.priority}
                  />
                </div>
              ),
            },
            {
              label: t('labels.insightType'),
              value: insight.type,
            },
            {
              label: t('plainText.insightId'),
              value: insight.sequenceNumber,
            },
            {
              label: t('labels.source'),
              value: filterMap.has(insight?.sourceName?.toLowerCase())
                ? titleCase({
                    text: t(filterMap.get(insight?.sourceName?.toLowerCase())),
                    language,
                  })
                : insight?.sourceName,
            },
          ].map(({ label, value, children }) => (
            <FlexColumn key={label} $isFlexGrow={false}>
              <StyledInput label={label} value={value} readOnly />
              {children}
            </FlexColumn>
          ))}
        </div>
      </InsightDetail>
      <InsightDetail
        headerIcon="assets"
        headerText={t('plainText.assetDetails')}
      >
        {[
          {
            label: t('twinExplorer.asset'),
            children: (
              <AssetLink
                // somehow tw doesn't recognize translate-y-[-100%]
                // so we use "css" prop coming from styled-components
                css={{ transform: 'translateY(-100%)' }}
                tw="w-full h-[28px] pl-[9px]"
                path={routes.portfolio_twins_view__siteId__twinId(
                  insight?.siteId,
                  insight?.twinId
                )}
                siteId={insight?.siteId}
                twinId={insight?.twinId}
                assetName={insight?.equipmentName ?? insight?.asset?.name}
              />
            ),
          },
          {
            label: _.capitalize(t('plainText.assetType')),
            value: modelId ? getModelDisplayName(modelId, translation) : '--',
          },
          {
            label: t('labels.site'),
            value: sites?.find((s) => s.id === insight.siteId)?.name ?? '',
          },
        ].map(({ label, value, children }) => (
          <FlexColumn key={label} $isFlexGrow={false}>
            <StyledInput label={label} value={value} readOnly />
            {children}
          </FlexColumn>
        ))}
      </InsightDetail>
    </Container>
  )
}

const Label = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.default,
}))

const FlexColumn = styled.div<{
  $isFlexGrow?: boolean
  $isMarginLeft?: boolean
}>(({ $isFlexGrow, $isMarginLeft }) => ({
  position: 'relative',
  flexDirection: 'column',
  flexGrow: $isFlexGrow ? 1 : 0,
  width: 'fit-content',
  height: '54px',
  marginLeft: $isMarginLeft ? '16px' : '0',

  '& > div': {
    width: '100%',
  },
}))

const StyledInput = styled(Input)({
  '& > input': {
    overflow: 'hidden',
    whiteSpace: 'nowrap',
    textOverflow: 'ellipsis',
  },
})
