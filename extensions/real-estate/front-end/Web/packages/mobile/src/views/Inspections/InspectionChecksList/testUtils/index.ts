import { v4 as uuidv4 } from 'uuid'
import {
  Check,
  Inspection,
  InspectionRecord,
  CheckWithoutInspectionId,
  CheckRecord,
} from '../types'

/**
 * Make an inspection record from an inspection. This is just a package
 * containing the inspection and empty check records for each check.
 */
export function makeInspectionRecord(inspection: Inspection): InspectionRecord {
  const inspectionRecordId = uuidv4()
  return {
    id: inspectionRecordId,
    inspectionId: inspection.id,
    inspection,
    checkRecords: inspection.checks.map((check) => ({
      id: uuidv4(),
      inspectionId: inspection.id,
      inspectionRecordId,
      checkId: check.id,
      status: 'overdue',
      effectiveDate: '2022-11-03T04:00:00.000Z',
      attachments: [],
      submittedUserId: '123',
      submittedDate: '123',
      submittedSiteLocalDate: '123',
    })),
  }
}

/**
 * Make an inspection from a list of checks.
 */
export function makeInspection({
  checks,
}: {
  checks: CheckWithoutInspectionId[]
}): Inspection {
  const inspectionId = uuidv4()

  return {
    id: inspectionId,
    name: 'ACU-L33-01-01',
    zoneId: '55d7b030-3d86-4c0d-af20-b4f876c09d17',
    assetId: '00600000-0000-0000-0000-000000734169',
    floorCode: 'L33',
    assignedWorkgroupId: '24ab188e-3419-4b9e-adcd-308e2a9809f4',
    sortOrder: 4,
    frequencyInHours: 8,
    startDate: '2022-11-02T15:30:00.000Z',
    endDate: '2022-11-24T00:00:00.000Z',
    nextEffectiveDate: '0001-01-01T00:00:00.000Z',
    checks: checks.map((c) => ({ ...c, inspectionId } as Check)),
  }
}

export const makeCheckRecord = (
  check: Check,
  inspectionRecordId: string,
  status: CheckRecord['status'],
  value?: number | string
): CheckRecord => {
  const base = {
    id: uuidv4(),
    inspectionId: check.inspectionId,
    checkId: check.id,
    inspectionRecordId,
    notes: '',
    attachments: [],
    status,
    effectiveDate: '',
  }
  if (check.type === 'numeric' || check.type === 'total') {
    return { ...base, numberValue: value as number }
  } else if (check.type === 'date') {
    return { ...base, dateValue: value as string }
  } else {
    return { ...base, stringValue: value as string }
  }
}
