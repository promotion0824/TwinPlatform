import { inspection } from './testUtils/inspection'
import { canOpenCheck } from '../InspectionCheck'

describe('InspectionCheck', () => {
  describe('canOpenCheck', () => {
    test('check is independent on other checks, so that it should be possible to open', () => {
      const checkRecord = {
        id: '6c3e8738-c46d-4c76-a466-8824b1071034',
        inspectionId: 'c919b1cf-8c43-47f0-884b-671a8073f0b1',
        checkId: '794b02c7-425d-490c-abea-1f5c48fb84fa',
        inspectionRecordId: '1204cf45-2c62-4241-aaac-2c6547bb1d11',
        status: 'overdue',
        effectiveDate: '2022-10-30T21:00:00.000Z',
        attachments: [],
      }
      const check = inspection.checks[0]
      const canOpen = canOpenCheck(inspection, check.id, checkRecord)
      expect(canOpen).toBe(true)
    })

    test('check is dependant on another check with value different from dependency value, so that it should not be possible to open', () => {
      const checkRecord = {
        id: '6c3e8738-c46d-4c76-a466-8824b1071034',
        inspectionId: 'c919b1cf-8c43-47f0-884b-671a8073f0b1',
        checkId: '794b02c7-425d-490c-abea-1f5c48fb84fa',
        inspectionRecordId: '1204cf45-2c62-4241-aaac-2c6547bb1d11',
        status: 'overdue',
        effectiveDate: '2022-10-30T21:00:00.000Z',
        attachments: [],
      }
      const check = inspection.checks[3]
      const canOpen = canOpenCheck(inspection, check.id, checkRecord)
      expect(canOpen).toBe(false)
    })

    test('check is dependant on another check with value equal to dependency value, so that it should be possible to open', () => {
      const checkRecord = {
        id: '6c3e8738-c46d-4c76-a466-8824b1071034',
        inspectionId: 'c919b1cf-8c43-47f0-884b-671a8073f0b1',
        checkId: '794b02c7-425d-490c-abea-1f5c48fb84fa',
        inspectionRecordId: '1204cf45-2c62-4241-aaac-2c6547bb1d11',
        status: 'overdue',
        effectiveDate: '2022-10-30T21:00:00.000Z',
        attachments: [],
      }
      const check = inspection.checks[4]
      const canOpen = canOpenCheck(inspection, check.id, checkRecord)
      expect(canOpen).toBe(true)
    })
  })
})
