import { generateTimeItems } from './generateTimeItems'
describe('generateTimeItems', () => {
  it('should generate time items from 00:00 to 23:59:59 at 30-minute intervals in "hh:mm:ss a" format', () => {
    const result = generateTimeItems({
      startTime: '00:00',
      endTime: '23:59:59',
      interval: 30 * 60 * 1000, // 30 minutes in milliseconds
      format: 'hh:mm:ss a',
    })

    expect(result.length).toEqual(48)
    expect(result[0]).toEqual({ value: '00:00:00', label: '12:00:00 am' })
    expect(result[10]).toEqual({ value: '05:00:00', label: '05:00:00 am' })
    expect(result[20]).toEqual({ value: '10:00:00', label: '10:00:00 am' })
    expect(result[30]).toEqual({ value: '15:00:00', label: '03:00:00 pm' })
    expect(result[40]).toEqual({ value: '20:00:00', label: '08:00:00 pm' })
    expect(result[47]).toEqual({ value: '23:30:00', label: '11:30:00 pm' })
  })
})
