import { SortBy, SourceType } from '../insights/insights/types'
import { groupAttachments } from './activityUtils'

describe('groupAttachments', () => {
  test('should group attachments within 1 minute for the same user and ticket', () => {
    const result = groupAttachments(
      [
        {
          ticketId: 'ticket-1',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-1',
          activityDate: '2023-07-21T06:45:00.000Z',
          activities: [{ key: '1', value: 'attachment-1' }],
        },
        {
          ticketId: 'ticket-1',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-1',
          activityDate: '2023-07-21T06:45:30.000Z',
          activities: [{ key: '2', value: 'attachment-2' }],
        },
      ],
      SortBy.asc
    )
    expect(result.length).toBe(1)
    expect(result[0].activities.length).toBe(2)
  })

  test('should not group attachments for a gap of more than 1 minute', () => {
    const result = groupAttachments(
      [
        {
          ticketId: 'ticket-2',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-2',
          activityDate: '2023-07-21T06:45:00.000Z',
          activities: [{ key: '3', value: 'attachment-3' }],
        },
        {
          ticketId: 'ticket-2',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-2',
          activityDate: '2023-07-21T06:46:30.000Z',
          activities: [{ key: '4', value: 'attachment-4' }],
        },
      ],
      SortBy.asc
    )
    expect(result.length).toBe(2)
    expect(result[0].activities.length).toBe(1)
    expect(result[1].activities.length).toBe(1)
  })

  test('should not group attachments for different tickets but the same user', () => {
    const result = groupAttachments(
      [
        {
          ticketId: 'ticket-3',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-3',
          activityDate: '2023-07-21T06:45:00.000Z',
          activities: [{ key: '5', value: 'attachment-5' }],
        },
        {
          ticketId: 'ticket-4',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-3',
          activityDate: '2023-07-21T06:45:30.000Z',
          activities: [{ key: '6', value: 'attachment-6' }],
        },
      ],
      SortBy.asc
    )
    expect(result.length).toBe(2)
    expect(result[0].activities.length).toBe(1)
    expect(result[1].activities.length).toBe(1)
  })

  test('should not group attachments for different users but the same ticket', () => {
    const result = groupAttachments(
      [
        {
          ticketId: 'ticket-5',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-4',
          activityDate: '2023-07-21T06:45:00.000Z',
          activities: [{ key: '7', value: 'attachment-7' }],
        },
        {
          ticketId: 'ticket-5',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-5',
          activityDate: '2023-07-21T06:45:30.000Z',
          activities: [{ key: '8', value: 'attachment-8' }],
        },
      ],
      SortBy.asc
    )
    expect(result.length).toBe(2)
    expect(result[0].activities.length).toBe(1)
    expect(result[1].activities.length).toBe(1)
  })

  test('should not group attachments for different tickets and users', () => {
    const result = groupAttachments(
      [
        {
          ticketId: 'ticket-6',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-6',
          activityDate: '2023-07-21T06:45:00.000Z',
          activities: [{ key: '9', value: 'attachment-9' }],
        },
        {
          ticketId: 'ticket-7',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-7',
          activityDate: '2023-07-21T06:45:30.000Z',
          activities: [{ key: '10', value: 'attachment-10' }],
        },
      ],
      SortBy.asc
    )
    expect(result.length).toBe(2)
    expect(result[0].activities.length).toBe(1)
    expect(result[1].activities.length).toBe(1)
  })

  test('should not group attachments for different activity types', () => {
    const result = groupAttachments(
      [
        {
          ticketId: 'ticket-8',
          sourceType: SourceType.platform,
          activityType: 'TicketAttachment',
          userId: 'user-8',
          activityDate: '2023-07-21T06:45:00.000Z',
          activities: [{ key: '11', value: 'attachment-11' }],
        },
        {
          ticketId: 'ticket-8',
          sourceType: SourceType.platform,
          activityType: 'InsightActivity',
          userId: 'user-8',
          activityDate: '2023-07-21T06:45:30.000Z',
          activities: [{ key: '12', value: 'activity-12' }],
        },
      ],
      SortBy.asc
    )
    expect(result.length).toBe(2)
    expect(result[0].activities.length).toBe(1)
    expect(result[1].activities.length).toBe(1)
  })

  test('should return an empty array for empty activities', () => {
    const result = groupAttachments([], SortBy.asc)
    expect(result).toEqual([])
  })
})
