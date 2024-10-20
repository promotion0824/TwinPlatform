import { NotFound } from '@willow/ui'
import { styled } from 'twin.macro'

const Inner = styled.div({
  maxWidth: '340px',
  textAlign: 'center',
  fontWeight: 'normal',
  whiteSpace: 'pre-line',
})

export default function NoTwinsFound({ t }) {
  return (
    <NotFound>
      <Inner>{t('interpolation.twinsFoundCount_zero')}</Inner>
    </NotFound>
  )
}
