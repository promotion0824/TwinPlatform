import { useState } from 'react'
import {
  useAnalytics,
  useApi,
  useSnackbar,
  useUser,
  Checkbox,
  Dropdown,
  DropdownContent,
  DropdownButton,
  useModal,
} from '@willow/mobile-ui'
import Avatar from 'components/Avatar/Avatar'
import styles from './UserProfile.css'
import ChangeRegionModal from './ChangeRegionModal'

export default function UserProfile() {
  const analytics = useAnalytics()
  const user = useUser()
  const api = useApi()
  const snackbar = useSnackbar()
  const [isChecked, setIsChecked] = useState(
    user?.preferences?.mobileNotificationEnabled
  )
  const modal = useModal()

  const handleMobileNotificationsChange = async (nextValue) => {
    // Optimistic update of UI
    setIsChecked(nextValue)
    await api
      .put('/api/me/preferences', {
        mobileNotificationEnabled: nextValue,
      })
      .catch(() => {
        setIsChecked(!nextValue)
        snackbar.show(
          'An error has occurred while updating mobile notifications setting.'
        )
      })
    if (nextValue) {
      analytics.track('Mobile Notifications Turned On')
    } else {
      analytics.track('Mobile Notifications Turned Off')
    }
  }

  const changeRegionClick = () => {
    modal.open(<ChangeRegionModal />)
  }

  return (
    <Dropdown showBorder={false} className={styles.dropdown}>
      <Avatar firstName={user.firstName} lastName={user.lastName} />
      <DropdownContent position="right" contentClassName={styles.content}>
        <div className={styles.notificationsWrapper}>
          <Checkbox
            checked={isChecked}
            label="Notifications"
            onChange={handleMobileNotificationsChange}
          />
        </div>
        <DropdownButton icon="notificationBell" className="beamerTrigger">
          <div>What's new</div>
        </DropdownButton>
        <>
          <DropdownButton
            closeOnClick={false}
            onClick={() => changeRegionClick()}
          >
            Change region
          </DropdownButton>
          <hr />
        </>
        <DropdownButton to="/account/logout" color={null}>
          Log Out
        </DropdownButton>
      </DropdownContent>
    </Dropdown>
  )
}
