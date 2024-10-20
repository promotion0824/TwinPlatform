export type DateTimeType =
  | 'date'
  | 'date-range'
  | 'date-time'
  | 'date-time-range'

export type DateTimeInputValue<T extends DateTimeType> = T extends
  | 'date'
  | 'date-time'
  ? Date
  : T extends 'date-range' | 'date-time-range'
  ? [Date, Date]
  : never

export type DateValue = Date | null
