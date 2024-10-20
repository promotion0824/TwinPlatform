import _ from 'lodash'
import { styled } from 'twin.macro'
import { useForm, Fieldset, Input } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { Creator } from '@willow/common/insights/insights/types'

export default function CreatorDetails() {
  const form: { data: { creator: Creator } } = useForm()
  const {
    data: { creator },
  } = form
  const { t } = useTranslation()

  return (
    <Fieldset icon="user" legend={t('plainText.creatorDetails')}>
      <FlexContainer flexFlow="row" flexShrink={0}>
        <FlexContainer flexFlow="column" flexGrow={1}>
          <Input
            name="creator.name"
            data-cy="creator-name"
            label={_.capitalize(t('labels.creator'))}
            readOnly
            value={creator?.name ?? '--'}
          />
        </FlexContainer>
        <FlexContainer flexFlow="column" marginLeft="16px" flexGrow={1}>
          <Input
            name="creator.mobile"
            data-cy="creator-phone"
            label={t('labels.contactNumber')}
            readOnly
            value={creator?.mobile ?? '--'}
          />
        </FlexContainer>
      </FlexContainer>
      <FlexContainer flexFlow="row" flexShrink={0}>
        <FlexContainer flexFlow="column" flexGrow={1}>
          <Input
            name="creator.email"
            data-cy="creator-email"
            label={t('labels.contactEmail')}
            readOnly
            value={creator?.email ?? '--'}
          />
        </FlexContainer>
        <FlexContainer flexFlow="column" flexGrow={1} marginLeft="16px">
          <Input
            name="creator.company"
            data-cy="creator-company"
            label={t('labels.company')}
            readOnly
            value={creator?.company ?? '--'}
          />
        </FlexContainer>
      </FlexContainer>
    </Fieldset>
  )
}

const FlexContainer = styled('div')<{
  flexFlow: string
  flexShrink?: number
  marginLeft?: string
  flexGrow?: number
}>(({ flexFlow, flexShrink, marginLeft, flexGrow }) => ({
  flexFlow,
  flexShrink,
  display: 'flex',
  marginLeft,
  flexGrow,

  '& > div': {
    width: '100%',
  },
}))
