import tw from 'twin.macro'
import {
  useForm,
  Fieldset,
  Flex,
  Label,
  Link,
  useScopeSelector,
} from '@willow/ui'
import { useSite } from 'providers'
import { useTranslation } from 'react-i18next'
import { makeInsightLink } from '../../../../routes'

export default function Insight() {
  const { isScopeSelectorEnabled, location } = useScopeSelector()
  const form = useForm()
  const site = useSite()
  const { t } = useTranslation()

  if (site.features.isInsightsDisabled || form.data.insightId == null) {
    return null
  }

  return (
    <Fieldset icon="default" legend={t('headers.insight')}>
      <Label label={t('labels.thisTicketIsLinkedToAnInsight')}>
        <Flex align="left" padding="small medium">
          <Link
            tw="underline"
            to={makeInsightLink({
              siteId: form.data?.siteId,
              insightId: form.data.insightId,
              scopeId: location?.twin?.id,
              isScopeSelectorEnabled,
            })}
          >
            {form.data.insightName}
          </Link>
        </Flex>
      </Label>
    </Fieldset>
  )
}
