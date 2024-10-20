import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { TwinChip } from '@willow/ui'
import { FieldSet } from './shared'
import { PartialModelOfInterest, FormMode } from '../../types'

/**
 * Preview section will display a TwinChip with all the user inputs from the MOI form.
 */
export default function PreviewSection({
  selectedModelOfInterest,
  formMode,
}: {
  selectedModelOfInterest?: PartialModelOfInterest
  formMode: FormMode
}) {
  const { t } = useTranslation()
  return (
    <FieldSet noIndent>
      {formMode === 'edit' && (
        <EditExistingMOIText>
          {t('plainText.editExistingMOI')}
        </EditExistingMOIText>
      )}
      <Preview selectedModelOfInterest={selectedModelOfInterest} />
    </FieldSet>
  )
}

function Preview({
  selectedModelOfInterest,
}: {
  selectedModelOfInterest?: PartialModelOfInterest
}) {
  const { t } = useTranslation()
  const { modelId } = selectedModelOfInterest || {}
  return (
    <BlackBackground>
      <Container>
        {modelId ? (
          <TwinChip modelOfInterest={selectedModelOfInterest} />
        ) : (
          <PreviewText>{t('plainText.previewSelection')}</PreviewText>
        )}
      </Container>
    </BlackBackground>
  )
}

const BlackBackground = styled.div({
  background: '#171717',
  width: '595px',
  height: '140px',
  margin: '5px 0px',
})

const Container = styled.div({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100%',
})

const EditExistingMOIText = styled.span({
  font: 'normal 500 10px/16px Poppins',
  color: '#959595',
  marginBottom: '15px',
})

const PreviewText = styled.span({
  font: '500 9px/13px Poppins',
  color: '#7E7E7E',
})
