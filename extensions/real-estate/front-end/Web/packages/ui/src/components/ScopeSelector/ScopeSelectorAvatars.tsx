import { Avatar, Icon } from '@willowinc/ui'

import getScopeSelectorModel from './getScopeSelectorModel'

export default function ScopeSelectorAvatar({ modelId }: { modelId: string }) {
  const model = getScopeSelectorModel(modelId)
  return (
    <Avatar color={model.color} shape="rectangle" variant="subtle">
      <Icon icon={model.icon} size={16} />
    </Avatar>
  )
}
