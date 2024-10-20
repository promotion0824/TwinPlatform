import { timesToListItems } from './timesToListItems'

describe('timesToListItems', () => {
  const sampleTimes = [
    '00:00:00',
    '06:00:00',
    '12:00:00',
    '18:00:00',
    '23:00:00',
  ]

  it('should format times using "HH:mm:ss"', () => {
    const result = timesToListItems({
      times: sampleTimes,
      format: 'HH:mm:ss',
    })
    expect(result).toEqual([
      { value: '00:00:00', label: '00:00:00' },
      { value: '06:00:00', label: '06:00:00' },
      { value: '12:00:00', label: '12:00:00' },
      { value: '18:00:00', label: '18:00:00' },
      { value: '23:00:00', label: '23:00:00' },
    ])
  })

  it('should format times using "hh:mm:ss a"', () => {
    const result = timesToListItems({
      times: sampleTimes,
      format: 'hh:mm:ss a',
    })
    expect(result).toEqual([
      { value: '00:00:00', label: '12:00:00 am' },
      { value: '06:00:00', label: '06:00:00 am' },
      { value: '12:00:00', label: '12:00:00 pm' },
      { value: '18:00:00', label: '06:00:00 pm' },
      { value: '23:00:00', label: '11:00:00 pm' },
    ])
  })

  it('should format times using "hh:mm:ss A"', () => {
    const result = timesToListItems({
      times: sampleTimes,
      format: 'hh:mm:ss A',
    })
    expect(result).toEqual([
      { value: '00:00:00', label: '12:00:00 AM' },
      { value: '06:00:00', label: '06:00:00 AM' },
      { value: '12:00:00', label: '12:00:00 PM' },
      { value: '18:00:00', label: '06:00:00 PM' },
      { value: '23:00:00', label: '11:00:00 PM' },
    ])
  })

  it('should format times using "HH:mm"', () => {
    const result = timesToListItems({
      times: sampleTimes,
      format: 'HH:mm',
    })
    expect(result).toEqual([
      { value: '00:00', label: '00:00' },
      { value: '06:00', label: '06:00' },
      { value: '12:00', label: '12:00' },
      { value: '18:00', label: '18:00' },
      { value: '23:00', label: '23:00' },
    ])
  })

  it('should format times using "hh:mm a"', () => {
    const result = timesToListItems({
      times: sampleTimes,
      format: 'hh:mm a',
    })
    expect(result).toEqual([
      { value: '00:00', label: '12:00 am' },
      { value: '06:00', label: '06:00 am' },
      { value: '12:00', label: '12:00 pm' },
      { value: '18:00', label: '06:00 pm' },
      { value: '23:00', label: '11:00 pm' },
    ])
  })

  it('should format times using "hh:mm A"', () => {
    const result = timesToListItems({
      times: sampleTimes,
      format: 'hh:mm A',
    })
    expect(result).toEqual([
      { value: '00:00', label: '12:00 AM' },
      { value: '06:00', label: '06:00 AM' },
      { value: '12:00', label: '12:00 PM' },
      { value: '18:00', label: '06:00 PM' },
      { value: '23:00', label: '11:00 PM' },
    ])
  })
})
