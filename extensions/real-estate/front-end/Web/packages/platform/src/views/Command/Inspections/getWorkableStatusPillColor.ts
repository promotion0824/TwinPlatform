import { CheckRecordStatus } from '../../../services/Inspections/InspectionsServices'

const getWorkableStatusPillColor = (status?: string) => {
  switch (status) {
    case CheckRecordStatus.Completed:
      return 'green'
    case CheckRecordStatus.Overdue:
    case CheckRecordStatus.Missed:
      return 'red'
    case CheckRecordStatus.Due:
    default:
      return 'orange'
  }
}

export default getWorkableStatusPillColor
