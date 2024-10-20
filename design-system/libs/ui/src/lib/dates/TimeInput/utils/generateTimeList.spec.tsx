import { generateTimeList } from './generateTimeList'

describe('generateTimeList', () => {
  it('should return correct list with interval of 30 minutes', () => {
    const timeList = generateTimeList({
      startTime: '00:00:00',
      endTime: '01:59:59',
      interval: 30 * 60 * 1000, // 30 minutes
    })
    expect(timeList).toEqual(['00:00:00', '00:30:00', '01:00:00', '01:30:00'])
  })

  it('should include endTime if it meets the interval', () => {
    const timeList = generateTimeList({
      startTime: '00:00:00',
      endTime: '01:30:00',
      interval: 30 * 60 * 1000, // 30 minutes
    })
    expect(timeList).toContain('01:30:00')
  })

  it('should return correct list with a interval not divisible by 10', () => {
    const timeList = generateTimeList({
      startTime: '00:00:00',
      endTime: '00:09:59',
      interval: 54321,
    })
    expect(timeList).toEqual([
      '00:00:00',
      '00:00:54',
      '00:01:48',
      '00:02:42',
      '00:03:37',
      '00:04:31',
      '00:05:25',
      '00:06:20',
      '00:07:14',
      '00:08:08',
      '00:09:03',
      '00:09:57',
    ])
  })

  it('should return correct list even startTime do not have seconds', () => {
    const timeList = generateTimeList({
      startTime: '00:00',
      endTime: '01:59:59',
      interval: 30 * 60 * 1000, // 30 minutes
    })
    expect(timeList).toEqual(['00:00:00', '00:30:00', '01:00:00', '01:30:00'])
  })

  it('should return correct list even both startTime and endTime do not have seconds', () => {
    const timeList = generateTimeList({
      startTime: '00:00',
      endTime: '01:59',
      interval: 30 * 60 * 1000, // 30 minutes
    })
    expect(timeList).toEqual(['00:00:00', '00:30:00', '01:00:00', '01:30:00'])
  })

  it('should return complete list with default startTime and endTime with interval 60 minutes', () => {
    const timeList = generateTimeList({
      interval: 60 * 60 * 1000, // 60 minutes
    })
    expect(timeList).toEqual([
      '00:00:00',
      '01:00:00',
      '02:00:00',
      '03:00:00',
      '04:00:00',
      '05:00:00',
      '06:00:00',
      '07:00:00',
      '08:00:00',
      '09:00:00',
      '10:00:00',
      '11:00:00',
      '12:00:00',
      '13:00:00',
      '14:00:00',
      '15:00:00',
      '16:00:00',
      '17:00:00',
      '18:00:00',
      '19:00:00',
      '20:00:00',
      '21:00:00',
      '22:00:00',
      '23:00:00',
    ])
  })
})
