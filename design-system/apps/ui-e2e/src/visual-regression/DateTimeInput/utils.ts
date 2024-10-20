import { expect, Page } from '@willowinc/playwright'
import dayjs from 'dayjs'

export const DEFAULT_PLACEHOLDER = 'Pick date'

export const getDateInputsContainer = async (page: Page) =>
  page.getByTestId('date-inputs-container')
export const getDateCell = async (page: Page, date: Date) =>
  page.getByRole('button', {
    name: dayjs(date).format('D MMMM YYYY'),
    exact: true,
  })
const toInputFormat = (date: Date) => dayjs(date).format('DD MMM YYYY')

export const expectDateIsSelected = async (page: Page, date: Date) =>
  await expect(
    (await getDateCell(page, date))
  ).toHaveAttribute('data-selected', 'true')
export const expectInputToHaveValue = async (
  page: Page,
  label: string,
  date?: Date
) => expect(page.getByLabel(label)).toHaveValue(date ? toInputFormat(date) : '')

export function getNthDayOfCurrentMonth(n: number) {
  const currentDate = new Date()
  return new Date(currentDate.getFullYear(), currentDate.getMonth(), n)
}

export async function toggleCalendar(page: Page) {
  return await page.getByPlaceholder(DEFAULT_PLACEHOLDER).click()
}

export async function applyChange(page: Page) {
  return page.getByRole('button', { name: 'Apply' }).click()
}
export async function cancelChange(page: Page) {
  return page.getByRole('button', { name: 'Cancel' }).click()
}
