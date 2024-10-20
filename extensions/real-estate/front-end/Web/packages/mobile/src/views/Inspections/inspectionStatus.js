const InspectionStatus = {}
InspectionStatus[(InspectionStatus.Due = 'due')] = 'Due'
InspectionStatus[(InspectionStatus.Overdue = 'overdue')] = 'Overdue'
InspectionStatus[(InspectionStatus.Completed = 'completed')] = 'Completed'
InspectionStatus[(InspectionStatus.Missed = 'missed')] = 'Missed'
InspectionStatus[(InspectionStatus.NotRequired = 'notRequired')] =
  'Not Required'

export default InspectionStatus
