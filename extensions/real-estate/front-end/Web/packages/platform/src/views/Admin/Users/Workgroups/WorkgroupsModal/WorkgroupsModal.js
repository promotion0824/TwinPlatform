import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { Fetch, Modal } from '@willow/ui'
import WorkgroupsForm from './WorkgroupsForm'

export default function WorkgroupsModal({ workgroup, sites, onClose }) {
  const { t } = useTranslation()
  return (
    <Modal header={t('headers.workgroup')} size="small" onClose={onClose}>
      {(modal) => (
        <Fetch url="/api/me/persons">
          {(users) => (
            <WorkgroupsForm
              workgroup={workgroup}
              sites={sites}
              users={_(users)
                .filter(
                  (user) =>
                    user.type === 'customerUser' && user.status === 'active'
                )
                .orderBy((user) =>
                  `${user.firstName} ${user.lastName}`.toLowerCase()
                )
                .value()}
              modal={modal}
            />
          )}
        </Fetch>
      )}
    </Modal>
  )
}
