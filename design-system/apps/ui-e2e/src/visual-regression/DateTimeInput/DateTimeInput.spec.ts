import dayjs from 'dayjs'

import { Page, expect, test } from '@willowinc/playwright'
import {
  expectDateIsSelected,
  expectInputToHaveValue,
  getDateCell,
  getDateInputsContainer,
  getNthDayOfCurrentMonth,
  DEFAULT_PLACEHOLDER,
  toggleCalendar,
  applyChange,
  cancelChange,
} from './utils'

const START_DATE = '1 January 2022'
const END_DATE = '1 January 2023'
const firstDayOfCurrentMonth = getNthDayOfCurrentMonth(1)
const twentiethDayOfCurrentMonth = getNthDayOfCurrentMonth(20)

const singleTypes = [
  { type: 'date', story: 'Date' },
  { type: 'date-time', story: 'DateTime' },
]
const rangeTypes = [
  { type: 'date-range', story: 'DateRange' },
  { type: 'date-time-range', story: 'DateTimeRange' },
]
const allTypes = [...singleTypes, ...rangeTypes]

const componentName = 'DateTimeInput'
const groupName = 'Dates'

test.describe('Should have correct layout', () => {
  allTypes.map(({ type, story }) =>
    test(type, async ({ storybook, page }) => {
      // hidden stories has fixed calendar date
      await storybook.gotoHidden(componentName, story)
      await toggleCalendar(page)

      await expect(await getDateInputsContainer(page)).toHaveScreenshot(
        `${type}-should-have-correct-layout.png`
      )
    })
  )

  test('horizontal layout with label width', async ({ storybook }) => {
    await storybook.goto(
      componentName,
      groupName,
      'HorizontalLayoutWithLabelWidth'
    )

    await expect(storybook.storyRoot).toHaveScreenshot()
  })
})

test.describe('Calendar should update after type into', () => {
  singleTypes.map(({ type, story }) =>
    test(`${type} input`, async ({ storybook, page }) => {
      await storybook.goto(componentName, groupName, story)
      await toggleCalendar(page)

      await page.getByLabel('date').type(START_DATE)

      await expectDateIsSelected(page, new Date(START_DATE))
    })
  )

  rangeTypes.map(({ type, story }) =>
    test(`${type} inputs`, async ({ storybook, page }) => {
      await storybook.gotoHidden(componentName, story)
      await toggleCalendar(page)

      await page.getByLabel('start date').type(START_DATE)
      await expectDateIsSelected(page, new Date(START_DATE))

      await page.getByLabel('end date').type(END_DATE)
      await expectDateIsSelected(page, new Date(END_DATE))
    })
  )
})

test.describe('Clicking calendar should update', () => {
  singleTypes.map(({ type, story }) =>
    test(`${type} input`, async ({ storybook, page }) => {
      await storybook.goto(componentName, groupName, story)
      await toggleCalendar(page)
      const dateCell = await getDateCell(page, firstDayOfCurrentMonth)
      await dateCell.click()

      await expectInputToHaveValue(page, 'date', firstDayOfCurrentMonth)
    })
  )

  rangeTypes.map(({ type, story }) =>
    test(`${type} select a bigger date before a smaller date`, async ({
      storybook,
      page,
    }) => {
      await storybook.goto(componentName, groupName, story)
      await toggleCalendar(page)

      await (await getDateCell(page, twentiethDayOfCurrentMonth)).click()
      await (await getDateCell(page, firstDayOfCurrentMonth)).click()

      await expectInputToHaveValue(page, 'start date', firstDayOfCurrentMonth)
      await expectInputToHaveValue(page, 'end date', twentiethDayOfCurrentMonth)
    })
  )
})

test.describe('Trigger input should have correct style', () => {
  const getTriggerComponent = async (page: Page, placeholderText: string) =>
    page.getByPlaceholder(placeholderText)
  const stories = [
    { story: 'Readonly', placeholder: DEFAULT_PLACEHOLDER },
    { story: 'Disabled', placeholder: DEFAULT_PLACEHOLDER },
    { story: 'Error', placeholder: DEFAULT_PLACEHOLDER },
  ]

  stories.map(({ story, placeholder }) =>
    test(story, async ({ storybook, page }) => {
      await storybook.gotoHidden(componentName, story)
      await page.setViewportSize({ width: 400, height: 200 })

      await expect(
        await getTriggerComponent(page, placeholder)
      ).toHaveScreenshot()
    })
  )

  test('Custom placeholder', async ({ storybook, page }) => {
    await storybook.gotoHidden(componentName, 'CustomPlaceholder')
    await page.setViewportSize({ width: 400, height: 200 })

    expect(
      await getTriggerComponent(page, 'Custom placeholder text')
    ).toBeTruthy()
  })
})

test.describe('DateTimeInput', () => {
  test('should be able to switch month after set a date via typing', async ({
    storybook,
    page,
  }) => {
    await storybook.goto(componentName, groupName, singleTypes[0].story)
    await toggleCalendar(page)

    await page.getByLabel('date').type('1 January 2022')
    const nextButton = page.waitForSelector('button[data-direction="next"]')
    await (await nextButton).click()

    expect(page.getByText('February 2022')).toBeTruthy()
  })

  test('should be able to switch month after set a date range via typing', async ({
    storybook,
    page,
  }) => {
    await storybook.goto(componentName, groupName, rangeTypes[0].story)
    await toggleCalendar(page)

    await page.getByLabel('start date').type('1 January 2022')
    await page.getByLabel('end date').type('4 January 2022')

    const nextButton = page.waitForSelector('button[data-direction="next"]')
    await (await nextButton).click()

    expect(page.getByText('February 2022')).toBeTruthy()
  })

  const INITIAL_LOCAL_TIME = '30 Jan 2024, 04:00 PM - 29 Feb 2024, 06:13 AM'
  test('should display initial date time range', async ({
    storybook,
    page,
  }) => {
    await storybook.goto(
      componentName,
      groupName,
      'ControlledDateTimeRangeInput'
    )

    await expect(page.getByPlaceholder('Pick date and time range')).toHaveValue(
      INITIAL_LOCAL_TIME
    )
  })

  test('should update date range when make a selection with apply', async ({
    storybook,
    page,
  }) => {
    await storybook.goto(
      componentName,
      groupName,
      'ControlledDateTimeRangeInput'
    )
    await toggleCalendar(page)

    await (await getDateCell(page, new Date('2024-02-29'))).click()
    await (await getDateCell(page, new Date('2024-02-01'))).click()
    await applyChange(page)

    await expect(page.getByPlaceholder('Pick date and time range')).toHaveValue(
      '01 Feb 2024, 04:00 PM - 29 Feb 2024, 06:13 AM'
    )
  })

  test('should revert change when cancel', async ({ storybook, page }) => {
    await storybook.goto(
      componentName,
      groupName,
      'ControlledDateTimeRangeInput'
    )
    await toggleCalendar(page)

    await (await getDateCell(page, new Date('2024-02-29'))).click()
    await (await getDateCell(page, new Date('2024-02-01'))).click()
    await cancelChange(page)

    await expect(page.getByPlaceholder('Pick date and time range')).toHaveValue(
      INITIAL_LOCAL_TIME
    )
  })

  test('should have default time range applied when make date selection', async ({
    storybook,
    page,
  }) => {
    await storybook.gotoHidden(componentName, rangeTypes[1].story)
    await toggleCalendar(page)

    await (await getDateCell(page, new Date('2024-02-29'))).click()
    await (await getDateCell(page, new Date('2024-02-01'))).click()
    await applyChange(page)

    await expect(page.getByPlaceholder('Pick date and time range')).toHaveValue(
      '01 Feb 2024, 12:00 AM - 29 Feb 2024, 11:59 PM'
    )
  })
})
test.describe('DatePicker range selection in DateTimeInput', () => {
  rangeTypes.map(({ type, story }) => {
    test.describe(`after select start date in ${type} DatePicker,`, () => {
      test.beforeEach(async ({ storybook, page }) => {
        await storybook.goto(componentName, groupName, story)
        await toggleCalendar(page)

        const firstDayDateCell = await getDateCell(page, firstDayOfCurrentMonth)
        await firstDayDateCell.click()
      })

      test('start date will be auto deselected and apply quickActions range when click option in quickActions', async ({
        page,
      }) => {
        await page.getByText('1 day').click()
        await expectInputToHaveValue(
          page,
          'start date',
          dayjs().subtract(1, 'day').toDate()
        )
        await expectInputToHaveValue(page, 'end date', new Date())
      })

      test('start date will be auto deselected when click start date input', async ({
        page,
      }) => {
        await page.getByLabel('start date').click()
        await expectInputToHaveValue(page, 'start date', undefined)
      })

      test('start date will be auto deselected when click end date input', async ({
        page,
      }) => {
        await page.getByLabel('end date').click()
        await expectInputToHaveValue(page, 'start date', undefined)
      })

      test('start date will be auto deselected when click timezone selector', async ({
        page,
      }) => {
        await page.getByRole('textbox', { name: 'Timezone' }).click()
        await expectInputToHaveValue(page, 'start date', undefined)
      })

      test('start date will be preserved when switch month', async ({
        page,
      }) => {
        await page
          .getByRole('button', {
            name: dayjs().format('MMMM YYYY'),
            exact: true,
          })
          .click()
        await page
          .getByRole('button', {
            name: dayjs().add(1, 'month').format('MMM'),
            exact: true,
          })
          .click()
        await expectInputToHaveValue(page, 'start date', firstDayOfCurrentMonth)
      })

      test('start date will be selected when both start date and end date clicked', async ({
        page,
      }) => {
        await (await getDateCell(page, twentiethDayOfCurrentMonth)).click()
        await expectInputToHaveValue(page, 'start date', firstDayOfCurrentMonth)
        await expectInputToHaveValue(
          page,
          'end date',
          twentiethDayOfCurrentMonth
        )
      })
    })
  })
})
