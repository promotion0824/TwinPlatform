import { useFloor } from '../../FloorContext'
import AdminTabsMenu from './AdminTabsMenu/AdminTabsMenu'
import UserTabsMenu from './UserTabsMenu/UserTabsMenu'

export default function TabsMenu({ ...rest }) {
  const floor = useFloor()

  return floor.isReadOnly ? (
    <UserTabsMenu {...rest} />
  ) : (
    <AdminTabsMenu {...rest} />
  )
}
