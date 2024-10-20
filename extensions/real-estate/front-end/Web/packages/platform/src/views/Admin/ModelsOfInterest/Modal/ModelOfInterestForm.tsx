import { Button, useModal, Portal } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'

import PreviewSection from './FormComponents/PreviewSection'
import ChooseMOISection from './FormComponents/ChooseMOISection'
import IconPreferencesSection from './FormComponents/IconPreferencesSection'
import ColorPreferencesSection from './FormComponents/ColorPreferencesSection'

import { useManageModelsOfInterest } from '../Provider/ManageModelsOfInterestProvider'
import { PartialModelOfInterest } from '../types'

/**
 * This form is used to add new MOI or edit existing MOI.
 * It contains 4 sections:
 *   1. Preview section will display the TwinChip with all the user inputs from the subsequent sections.
 *   2. Choose MOI section will have inputs for selecting a model where its display name will be used in TwinChip's name.
 *   3. Icon Preferences section will have input for choosing the icon of the TwinChip.
 *   4. Color Preference section will have a list of color where users can choose the color of the icon for the TwinChip.
 */
export default function ModelOfInterestForm() {
  const { t } = useTranslation()

  const {
    selectedModelOfInterest,
    setSelectedModelOfInterest,
    formMode,
    control,
    submit,

    postModelOfInterestMutation,
    putModelOfInterestMutation,

    setShowConfirmDeleteModal,
    handleRevertChange,
  } = useManageModelsOfInterest()

  if (selectedModelOfInterest == null) {
    throw new Error(
      'ModelOfInterestForm expected selectedModelOfInterest to be defined'
    )
  }

  const { color: selectedColor, text: selectedText } = selectedModelOfInterest

  function handleColorChange(color: string) {
    setSelectedModelOfInterest((modelOfInterest) => ({
      ...modelOfInterest,
      color,
    }))
  }

  function handleIconChange(text: string) {
    setSelectedModelOfInterest((prev) => ({ ...prev, text }))
  }

  return (
    <>
      <FormContainer>
        <ModalButtons
          formMode={formMode}
          loading={
            postModelOfInterestMutation.isLoading ||
            putModelOfInterestMutation.isLoading
          }
          successful={
            postModelOfInterestMutation.isSuccess ||
            putModelOfInterestMutation.isSuccess
          }
          error={
            postModelOfInterestMutation.isError ||
            putModelOfInterestMutation.isError
          }
          onClick={submit}
          handleRevertChange={handleRevertChange}
        >
          {t('plainText.save')}
        </ModalButtons>
        <PreviewSection
          selectedModelOfInterest={selectedModelOfInterest}
          formMode={formMode}
        />

        <Container>
          <ChooseMOISection
            selectedModelOfInterest={selectedModelOfInterest}
            setSelectedModelOfInterest={setSelectedModelOfInterest}
            control={control}
            formMode={formMode}
          />

          <IconPreferencesSection
            selectedText={selectedText ?? ''}
            onChange={handleIconChange}
            control={control}
          />

          <ColorPreferencesSection
            selectedColor={selectedColor}
            onChange={handleColorChange}
          />

          {formMode === 'edit' && (
            <DeleteButtonContainer>
              <MarginButton
                color="red"
                onClick={() => {
                  setShowConfirmDeleteModal(true)
                }}
              >
                {t('plainText.deleteMOI')}
              </MarginButton>
            </DeleteButtonContainer>
          )}
        </Container>
      </FormContainer>
    </>
  )
}

const FormContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
})

const Container = styled.div({ height: '64vh', overflow: 'auto' })

const DeleteButtonContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  height: '165px',
})

const MarginButton = styled(Button)({
  marginTop: '25px',
  padding: '8px 40px',
  font: '600 11px/16px Poppins',
})

function ModalButtons({ formMode, handleRevertChange, ...rest }) {
  const modal = useModal()
  const { t } = useTranslation()

  return (
    <Portal target={modal.modalSubmitButtonRef}>
      <ButtonContainer>
        {formMode === 'edit' && (
          <RevertChangeButton color="transparent" onClick={handleRevertChange}>
            {t('plainText.revertChanges')}
          </RevertChangeButton>
        )}
        <Button color="purple" type="submit" {...rest}>
          {t('plainText.save')}
        </Button>
      </ButtonContainer>
    </Portal>
  )
}

const ButtonContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  font: '600 11px/16px Poppins',
})

const RevertChangeButton = styled(Button)({
  marginRight: '15px',
  padding: '8px 24px',
})
