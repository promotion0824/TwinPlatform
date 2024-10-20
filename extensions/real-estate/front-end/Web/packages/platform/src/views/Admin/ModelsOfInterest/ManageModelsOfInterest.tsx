import { DocumentTitle } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import SplitHeaderPanel from '../../Layout/Layout/SplitHeaderPanel'
import AdminTabs from '../AdminTabs'
import DeleteModelOfInterestModal from './Modal/DeleteModelOfInterestModal'
import ModelOfInterestModal from './Modal/ModelOfInterestModal'
import ModelsOfInterestTable from './ModelsOfInterestTable'
import ManageModelsOfInterestProvider, {
  useManageModelsOfInterest,
} from './Provider/ManageModelsOfInterestProvider'

/**
 * ManageModelsOfInterest is a presentation component where customer admins can see the set of model of interest,
 * and decide whether to modify them or add new ones.
 */
export default function ManageModelsOfInterest() {
  return (
    <ManageModelsOfInterestProvider>
      <ManageModelsOfInterestContent />
    </ManageModelsOfInterestProvider>
  )
}

function ManageModelsOfInterestContent() {
  const { t } = useTranslation()

  const {
    selectedModelOfInterest,
    setSelectedModelOfInterest,
    setFormMode,
    formMode,
    showConfirmDeleteModal,
    setShowConfirmDeleteModal,
    existingModelOfInterest,
  } = useManageModelsOfInterest()

  const defaultColor = '#33CA36'

  return (
    <>
      <DocumentTitle
        scopes={[t('plainText.modelsOfInterest'), t('headers.admin')]}
      />

      <SplitHeaderPanel
        leftElement={<AdminTabs />}
        rightElement={
          // Add button will open modal when clicked, to add new model of interest.
          <Button
            onClick={() => {
              setSelectedModelOfInterest({
                modelId: undefined,
                name: undefined,
                text: undefined,
                color: defaultColor,
              })
              setFormMode('add')
            }}
            prefix={<Icon icon="add" />}
          >
            {t('plainText.addModelsOfInterest')}
          </Button>
        }
      />

      <ModelsOfInterestTable />

      {selectedModelOfInterest && (
        <ModelOfInterestModal
          formMode={formMode}
          onClose={() => {
            setSelectedModelOfInterest(undefined)
            setFormMode(null)
          }}
        />
      )}

      {showConfirmDeleteModal && existingModelOfInterest && (
        <DeleteModelOfInterestModal
          modelName={existingModelOfInterest.name}
          onClose={() => {
            setShowConfirmDeleteModal(false)
          }}
        />
      )}
    </>
  )
}
