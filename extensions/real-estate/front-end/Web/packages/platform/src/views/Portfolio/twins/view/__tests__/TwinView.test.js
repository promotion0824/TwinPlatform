/* eslint-disable import/prefer-default-export */
import '@testing-library/jest-dom'
import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TicketStatusesStubProvider } from '@willow/common'
import {
  closeDropdown,
  getDropdownContent,
  openDropdown,
  supportDropdowns,
} from '@willow/ui/utils/testUtils/dropdown'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import _ from 'lodash'
import { matchRequestUrl, rest } from 'msw'
import { act } from 'react-dom/test-utils'

import { setupTestServer } from '../../../../../mockServer/testServer'
import {
  makeHandlers as makeTwinHandlers,
  makeTwinEtag,
} from '../../../../../mockServer/twins'
import { withoutRegion } from '../../../../../mockServer/utils'
import SearchResultsProvider from '../../results/page/state/SearchResults.js'
import {
  makeBooleanField,
  makeDoubleField,
  makeObjectField,
  makeObjectProperty,
  makeProperty,
  makeStringField,
  makeStringProperty,
  modelFromSchemas,
  objectToJsonPatch,
} from '../testUtils'
import { TwinEditorProvider } from '../TwinEditorContext.tsx'
import { TwinViewInner } from '../TwinView'
import { TwinViewProvider } from '../TwinViewContext'
import { testTwinVersions } from './testUtils/versionHistory'

// Don't test the right panel as part of this
jest.mock('../TwinViewRightPanelContainer', () => () => null)

supportDropdowns()
const { server, reset } = setupTestServer()

beforeAll(() => server.listen())
afterEach(() => {
  reset()
  server.resetHandlers()
  server.events.removeAllListeners()
})
afterAll(() => server.close())

const testTwin = {
  etag: makeTwinEtag(),
  name: 'Hello',
  updateThis: 'update this',
  doNotUpdateThis: 'no update pls',
  emptyValue: '',

  siteID: '123',
  id: '123',
  uniqueID: 'read only',

  group: {
    hello: 'string in group',
    booleanValue: true,
    subGroup: {
      sub: 'group',
      numberInSubgroup: 7,
    },
  },

  tags: {
    myTag: '123',
  },

  metadata: {
    modelId: 'myModelId',
  },
}

const testModel = {
  '@id': 'myModelId',
  displayName: 'My model',
  extends: [],
  contents: [
    makeStringProperty('name'),
    makeStringProperty('updateThis', {
      displayName: 'Update this',
    }),
    makeStringProperty('doNotUpdateThis', {
      displayName: 'Do not update this',
    }),
    makeStringProperty('nullValue', { displayName: 'Null value' }),
    makeStringProperty('emptyValue', {
      displayName: 'Empty value',
    }),
    makeStringProperty('siteID', { displayName: 'Site ID' }),
    makeStringProperty('uniqueID', { displayName: 'Unique ID' }),
    makeObjectProperty(
      'group',
      [
        makeStringField('hello', { displayName: 'Hello' }),
        makeBooleanField('booleanValue', { displayName: 'A boolean value' }),
        makeObjectField(
          'subGroup',
          [
            makeStringField('sub', { displayName: 'sub' }),
            makeDoubleField('numberInSubgroup', {
              displayName: 'Number in subgroup',
            }),
          ],
          {
            displayName: 'A sub group',
          }
        ),
      ],
      {
        displayName: 'A group',
      }
    ),
    makeProperty(
      'tags',
      {
        '@type': 'Map',
        mapKey: {
          name: 'tagName',
          schema: 'string',
        },
        mapValue: {
          name: 'tagValue',
          schema: 'string',
        },
      },
      {
        displayName: 'Tags',
      }
    ),
  ],
}

const testModelWithCustomProperties = {
  ...testModel,
  contents: [
    ...testModel.contents,
    makeProperty(
      'customProperties',
      {
        '@type': 'Map',
        mapKey: {
          name: 'sourceName',
          schema: 'string',
        },
        mapValue: {
          name: 'sourceProperties',
          schema: {
            '@type': 'Map',
            mapKey: {
              name: 'propertyName',
              schema: 'string',
            },
            mapValue: {
              name: 'propertyValue',
              schema: 'string',
            },
          },
        },
      },
      {
        displayName: 'Custom Properties',
      }
    ),
  ],
}

const testModelWithComponent = {
  ...testModel,
  contents: [
    ...testModel.contents,
    {
      '@type': 'Component',
      displayName: {
        en: 'fan',
      },
      name: 'fan',
      schema: {
        '@type': 'Object',
        fields: [
          {
            name: 'someFanField',
            schema: 'string',
          },
        ],
      },
    },
  ],
}

function Wrapper({ children }) {
  return (
    <BaseWrapper
      i18nOptions={{
        resources: {
          en: {
            translation: {
              'plainText.unnamedTwin': 'Unnamed twin',
              'plainText.keepOrUpdate': 'Modified value',
              'plainText.mustBeInteger': 'Must be an integer',
              'plainText.numberOutOfRange': 'Number is out of range',
              'plainText.mustBeNumber': 'Must be a number',
            },
          },
        },
        lng: 'en',
        fallbackLng: ['en'],
      }}
      user={{ isCustomerAdmin: true }}
      hasFeatureToggle={(feature) => feature !== 'cognitiveSearch'}
      sites={[
        {
          id: 'site1',
        },
      ]}
    >
      <TicketStatusesStubProvider>{children}</TicketStatusesStubProvider>
    </BaseWrapper>
  )
}

/**
 * Renders the twin editor for the specified twin and model. The twin's
 * metadata.modelId should match the model's ID. If it doesn't the twin will be
 * mutated to make it match.
 */
async function setup({
  twin = testTwin,
  restrictedFields,
  waitForEditButton = true,
  model = testModel,
} = {}) {
  if (twin.siteID == null) {
    throw new Error('twin must have a siteID')
  }

  if (twin.metadata == null) {
    twin.metadata = {} // eslint-disable-line no-param-reassign
  }
  twin.metadata.modelId = model['@id'] // eslint-disable-line no-param-reassign

  const submitRequests = []

  const serverTwinState = { [twin.id]: _.cloneDeep(twin) }

  const twinHandlers = makeTwinHandlers(serverTwinState).map(withoutRegion)

  const makeRelationships = (twinId) => [
    {
      source: {
        id: twinId,
        metadata: {
          modelId: 'myModelId',
        },
        siteID: '404bd33c-a697-4027-b6a6-677e30a53d07',
        name: 'Your twin',
      },
      target: {
        id: 'RELATIONSHIP-1',
        metadata: {
          modelId: 'myModelId',
        },
        siteID: '404bd33c-a697-4027-b6a6-677e30a53d07',
        name: 'Relationship 1',
      },
    },
  ]

  server.events.on('request:start', (req) => {
    if (
      req.method.toLowerCase() === 'patch' &&
      matchRequestUrl(req.url, `/api/sites/${twin.siteID}/twins/${twin.id}`)
        .matches
    ) {
      submitRequests.push(req)
    }
  })
  server.use(
    // Note: no /:region/ in these tests
    rest.get('/api/sites/:siteId/models', async (req, res, ctx) =>
      res(
        ctx.json([
          {
            descriptions: {},
            displayNames: { en: 'Some model' },
            id: model['@id'],
            isDecommissioned: false,
            isShared: false,
            model: JSON.stringify(model),
            uploadTime: 'some string, honestly who cares',
          },
        ])
      )
    ),

    rest.get('/api/v2/twins/:twinId/relationships', async (req, res, ctx) => {
      const { twinId } = req.params
      return res(ctx.json(makeRelationships(twinId)))
    }),
    rest.get(
      '/api/sites/:siteId/twins/:twinId/relationships',
      async (req, res, ctx) => {
        const { twinId } = req.params
        return res(ctx.json(makeRelationships(twinId)))
      }
    ),

    rest.get('/api/sites/:siteId/assets/:assetId', async (req, res, ctx) =>
      res(ctx.json({ hasLiveData: false }))
    ),

    rest.get('/api/sites/:siteId/twinGraph', async (req, res, ctx) =>
      res(ctx.json({ nodes: [], edges: [] }))
    ),

    rest.get('/api/v2/twins/:twinId/relatedTwins', async (req, res, ctx) =>
      res(ctx.json({ nodes: [], edges: [] }))
    ),
    rest.get(
      '/api/sites/:siteId/twins/:twinId/relatedTwins',
      async (req, res, ctx) => res(ctx.json({ nodes: [], edges: [] }))
    ),

    rest.get(
      '/api/customers/:customerId/modelsOfInterest',
      async (req, res, ctx) => res(ctx.json([]))
    ),

    rest.get(`/api/sites/:siteId/twins/:twinId/history`, (_req, res, ctx) =>
      res(ctx.json(testTwinVersions))
    ),

    // mocks to avoid unhandled requests
    rest.get(
      `/api/sites/:siteId/assets/:assetId/tickets/history`,
      (_req, res, ctx) => res(ctx.json([]))
    ),

    rest.get(
      `/api/sites/:siteId/assets/:assetId/pinOnLayer`,
      (_req, res, ctx) => res(ctx.json([]))
    ),

    rest.post('/api/insights', (_req, res, ctx) =>
      res(ctx.json({ items: [] }))
    ),

    ...twinHandlers
  )

  if (restrictedFields != null) {
    server.use(
      rest.get('/api/sites/:siteId/twins/restrictedFields', (req, res, ctx) =>
        res(ctx.json(restrictedFields))
      )
    )
  }

  function updateTwin(data) {
    serverTwinState[twin.id] = {
      ...serverTwinState[twin.id],
      ...data,
      etag: makeTwinEtag(),
    }
  }

  act(() => {
    render(
      <SearchResultsProvider>
        <TwinViewProvider>
          <TwinEditorProvider
            user={{ customer: { name: 'Bob' } }}
            siteId={twin.siteID}
            twinId={twin.id}
          >
            <TwinViewInner />
          </TwinEditorProvider>
        </TwinViewProvider>
      </SearchResultsProvider>,
      { wrapper: Wrapper }
    )
  })

  if (waitForEditButton) {
    await waitFor(() =>
      expect(screen.getByTitle('Edit twin information')).toBeInTheDocument()
    )
  }

  return {
    submitRequests,
    updateTwin,
  }
}

/**
 * Save, and wait for the save request to be complete (whether successful or
 * not).
 */
async function save() {
  act(() => {
    fireEvent.click(screen.getByTitle('Save'))
  })

  // Need to separately wait for the progress spinner to come and go, otherwise
  // we can miss.
  await waitFor(() => expect(screen.queryByTitle('Save')).toBeNull())
  await waitFor(() => expect(screen.queryByTitle('Saving')).toBeNull())
}

describe('TwinView tests', () => {
  test('Happy path', async () => {
    const { submitRequests } = await setup()

    fireEvent.click(screen.getByTitle('Edit twin information'))

    // The fields with values should be visible, the fields without values should
    // not be visible.
    expect(screen.queryByText('Update this')).not.toBeNull()
    expect(screen.queryByText('Empty value')).toBeNull()
    expect(screen.queryByText('Null value')).toBeNull()

    fireEvent.click(screen.getByText('Show more'))

    // Now we've shown more, the fields with empty values should also be visible.
    expect(screen.queryByText('Empty value')).not.toBeNull()
    expect(screen.queryByText('Null value')).not.toBeNull()

    // Update a field with a value and a field without a value.
    const updateThisTextBox = screen.getByLabelText('Update this')
    userEvent.clear(updateThisTextBox)
    userEvent.type(updateThisTextBox, 'updated value')

    const emptyValueTextBox = screen.getByLabelText('Empty value')
    userEvent.clear(emptyValueTextBox)
    userEvent.type(emptyValueTextBox, 'not empty any more!')

    fireEvent.click(screen.getByText('Show less'))

    // Assert that we JSON-stringified the subgroup object into the text box.
    expect(JSON.parse(screen.getByLabelText('A sub group').value)).toEqual({
      sub: 'group',
      numberInSubgroup: 7,
    })

    expect(screen.queryByText('Null value')).toBeNull()
    for (const field of ['Site ID', 'ID', 'Unique ID']) {
      userEvent.type(screen.getByLabelText(field), 'this does nothing')
    }

    await save()

    await waitFor(() => expect(submitRequests.length).toEqual(1))
    delete submitRequests[0].body.etag
    // We should send through the fields that we changed and no more.
    expect(submitRequests[0].body).toIncludeSameMembers(
      objectToJsonPatch({
        updateThis: 'updated value',
        emptyValue: 'not empty any more!',
      })
    )
  }, 10000)

  test('Add extra properties', async () => {
    const { submitRequests } = await setup()

    userEvent.click(screen.getByTitle('Edit twin information'))

    userEvent.click(screen.getByText('Show more'))

    const nullValueTextBox = screen.getByLabelText('Null value')
    userEvent.type(nullValueTextBox, 'not null anymore!')

    // Test also that we don't lose the new property even if we hit "Show less"
    userEvent.click(screen.getByText('Show less'))

    expect(screen.queryByLabelText('Null value')).not.toBeNull()

    await save()

    await waitFor(() => submitRequests.length === 1)
    expect(submitRequests[0].body).toContainEqual({
      op: 'add',
      path: '/nullValue',
      value: 'not null anymore!',
    })
  })

  test('Adding a new field to a twin with a null group', async () => {
    // Regression test: make sure that injecting a null value into a
    // previously- empty group does not break rendering.
    await setup({
      twin: {
        etag: makeTwinEtag(),
        siteID: '123',
        metadata: {
          modelId: 'my-model',
        },
      },
      model: modelFromSchemas({
        x: 'string',
        siteID: 'string',
        manufacturedByRef: {
          '@type': 'Object',
          fields: [
            {
              name: 'targetId',
              schema: 'string',
            },
            {
              name: 'name',
              schema: 'string',
            },
            {
              name: 'targetModelId',
              schema: 'string',
            },
          ],
        },
      }),
    })

    await new Promise((resolve) => setTimeout(resolve, 1000))

    userEvent.click(screen.getByTitle('Edit twin information'))

    userEvent.click(screen.getByText('Show more'))

    const nullValueTextBox = screen.getByLabelText('x')
    userEvent.type(nullValueTextBox, 'some value')

    // Just make sure the save succeeds and the subsequent rendering does not
    // crash.
    await save()
  })

  test('Delete properties', async () => {
    // Make sure deleted property gets hidden on save.
    const { submitRequests } = await setup()

    userEvent.click(screen.getByTitle('Edit twin information'))

    const updateThisTextBox = screen.getByLabelText('Update this')
    userEvent.clear(updateThisTextBox)

    await save()

    await waitFor(() => submitRequests.length === 1)

    expect(screen.queryByText('Update this')).toBeNull()
  })

  test('Should hide expanded null values on save', async () => {
    await setup()

    userEvent.click(screen.getByTitle('Edit twin information'))

    userEvent.click(screen.getByText('Show more'))
    expect(screen.queryByText('Null value')).not.toBeNull()

    await save()
    expect(screen.queryByText('Null value')).toBeNull()
  })

  test('Do not submit unchanged initially-absent properties', async () => {
    // When we click "Show more", the form will fill in default values for the
    // fields that didn't have values. If these values are objects, the user is
    // not currently able to change or remove them. So if we submit all the
    // properties in the form as they are, we will end up filling in values for
    // fields that the user didn't intend to fill in. To prevent this we only
    // submit properties that were either already there (in `existingFields`)
    // or which the user actually modified (in `dirtyFields`).

    const { submitRequests } = await setup({
      twin: _.omit(testTwin, ['group']),
    })

    fireEvent.click(screen.getByTitle('Edit twin information'))

    fireEvent.click(screen.getByText('Show more'))

    await save()

    // Make sure we didn't mess with the types of currently non-editable values
    // (booleans, sub-groups).
    await waitFor(() => submitRequests.length === 1)

    expect(submitRequests[0].body.some((op) => op.path === '/group')).toBe(
      false
    )
  })

  test("Re-hide expanded fields on save even if we didn't change anything", async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        etag: testTwin.etag,
      },
      model: testModelWithCustomProperties,
    })

    fireEvent.click(screen.getByTitle('Edit twin information'))
    fireEvent.click(screen.getByText('Show more'))
    await save()

    await waitFor(() => {
      expect(screen.queryByText('Custom Properties')).not.toBeInTheDocument()
    })
  })

  test('Cancel', async () => {
    await setup()

    fireEvent.click(screen.getByTitle('Edit twin information'))

    const updateThisTextBox = screen.getByLabelText('Update this')
    userEvent.clear(updateThisTextBox)
    userEvent.type(updateThisTextBox, "can't wait to lose this")

    act(() => {
      fireEvent.click(screen.getByTitle('Cancel'))
    })

    // Assert that we reverted to the old value.
    expect(screen.queryByText('Update this')).not.toBeNull()
    expect(screen.queryByText("can't wait to lose this")).toBeNull()
  })

  describe('Concurrent editing tests', () => {
    test('Handle another user changing data', async () => {
      const { submitRequests, updateTwin } = await setup()

      fireEvent.click(screen.getByTitle('Edit twin information'))

      fireEvent.click(screen.getByText('Show more'))

      // Make a local update
      const updateThisTextBox = screen.getByLabelText('Update this')
      userEvent.clear(updateThisTextBox)
      userEvent.type(updateThisTextBox, "can't wait to lose this")

      const otherTextBox = screen.getByLabelText('Do not update this')
      userEvent.clear(otherTextBox)
      userEvent.type(otherTextBox, 'not going to lose this')

      // Simulate another user updating the twin. This will update the etag.
      updateTwin({
        updateThis: 'yep you lost this alright',
      })

      await save()
      await waitFor(() => expect(submitRequests).toHaveLength(1))

      // Expect our value to be overwritten by the other user's value
      await waitFor(() =>
        expect(screen.getByLabelText('Update this')).toHaveValue(
          'yep you lost this alright'
        )
      )
      expect(screen.getAllByText('Modified value')).toHaveLength(1)

      // Regression test: make sure doing a merge does not lose our expanded
      // fields.
      expect(screen.getByLabelText('Null value')).toBeInTheDocument()

      // But now we have loaded the latest etag so we can save successfully
      await save()

      await waitFor(() => expect(submitRequests).toHaveLength(2))

      expect(submitRequests[1].body).toEqual([
        {
          op: 'replace',
          path: '/doNotUpdateThis',
          value: 'not going to lose this',
        },
      ])

      // Make sure we cleared the merge state when we saved.
      fireEvent.click(screen.getByTitle('Edit twin information'))
      expect(screen.queryByText('Modified value')).toBeNull()
    })

    test('Handle setting a merged field back to its original value', async () => {
      // Scenario:
      // 1. User A begins editing
      // 2. User B edits a field and saves
      // 3. User A saves, getting a merged conflict
      // 4. User A sets the field back to its original value from step 1
      // 5. User A saves
      // The value should be set to the value the user set in step 4. This will
      // break if we don't consider user B's value when saving.
      const { submitRequests, updateTwin } = await setup()

      fireEvent.click(screen.getByTitle('Edit twin information'))

      fireEvent.click(screen.getByText('Show more'))

      const updateThisTextBox = screen.getByLabelText('Update this')

      updateTwin({
        updateThis: 'value 2',
      })

      await save()
      await waitFor(() => expect(submitRequests).toHaveLength(1))

      await waitFor(() =>
        expect(screen.getByLabelText('Update this')).toHaveValue('value 2')
      )

      userEvent.clear(updateThisTextBox)
      userEvent.type(updateThisTextBox, 'update this')

      await save()

      await waitFor(() => expect(submitRequests).toHaveLength(2))
      expect(submitRequests[1].body).toEqual([
        {
          op: 'replace',
          path: '/updateThis',
          value: 'update this',
        },
      ])
    })

    test('Reload changed twin if we get a conflict and then cancel', async () => {
      const { updateTwin } = await setup()

      fireEvent.click(screen.getByTitle('Edit twin information'))

      const updateThisTextBox = screen.getByLabelText('Update this')
      userEvent.clear(updateThisTextBox)
      userEvent.type(updateThisTextBox, "can't wait to lose this")

      // Simulate another user updating the twin. This will update the etag.
      updateTwin({
        updateThis: 'yep you lost this alright',
      })

      await save()
      await waitFor(() =>
        expect(screen.getAllByText('Modified value')).toHaveLength(1)
      )

      act(() => {
        fireEvent.click(screen.getByTitle('Cancel'))
      })

      await waitFor(() =>
        expect(
          screen.getByText('yep you lost this alright')
        ).toBeInTheDocument()
      )
    })
  })

  test('Cancel after saving', async () => {
    // Test that if we save, then edit again, then cancel, we revert back to
    // the saved value, not the initial value.
    const { submitRequests } = await setup()

    userEvent.click(screen.getByTitle('Edit twin information'))

    userEvent.click(screen.getByText('Show more'))

    const updateThisTextBox = screen.getByLabelText('Update this')
    userEvent.clear(updateThisTextBox)
    userEvent.type(updateThisTextBox, 'I updated this')

    const nullValueTextBox = screen.getByLabelText('Null value')
    userEvent.clear(nullValueTextBox)
    userEvent.type(nullValueTextBox, 'Also keep this')

    await save()

    userEvent.click(screen.getByTitle('Edit twin information'))

    userEvent.click(screen.getByTitle('Cancel'))

    await waitFor(() => submitRequests.length === 2)
    expect(screen.queryByText('I updated this')).not.toBeNull()
    expect(screen.queryByText('Also keep this')).not.toBeNull()
  })

  test('Hidden properties', async () => {
    await setup({
      restrictedFields: {
        hiddenFields: ['/nullValue'],
        readOnlyFields: [],
      },
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    act(() => {
      fireEvent.click(screen.getByText('Show more'))
    })

    expect(screen.getByLabelText('Empty value')).toBeInTheDocument()
    expect(screen.queryByLabelText('Null value')).toBeNull()
  })

  test('Enum value dropdown options', async () => {
    // Test that we generate the correct labels for the dropdown options for
    // enum values. The label could come from the name or displayName, and the
    // displayName could be a string, or a mapping of languages to strings.
    await setup({
      twin: {
        etag: makeTwinEtag(),
        siteID: '123',
        metadata: {
          modelId: 'my-model',
        },
      },
      model: modelFromSchemas({
        siteID: 'string',
        withOnlyNames: {
          '@type': 'Enum',
          enumValues: [
            {
              enumValue: 'Wing',
              name: 'Wing',
            },
            {
              enumValue: 'Tower',
              name: 'Tower',
            },
          ],
          valueSchema: 'string',
        },
        withStringDisplayNames: {
          '@type': 'Enum',
          valueSchema: 'string',
          enumValues: [
            {
              name: 'Up',
              displayName: 'Up',
              enumValue: 'Up',
            },
            {
              name: 'Down',
              displayName: 'Down',
              enumValue: 'Down',
            },
          ],
        },
        withDictionaryDisplayNames: {
          '@type': 'Enum',
          valueSchema: 'string',
          enumValues: [
            {
              name: 'undefined',
              displayName: {
                en: 'undefined',
              },
              enumValue: 'undefined',
            },
            {
              name: 'analog',
              displayName: {
                en: 'analog',
              },
              enumValue: 'analog',
            },
          ],
        },
      }),
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    fireEvent.click(screen.getByText('Show more'))

    act(() => {
      openDropdown(screen.getByLabelText('withOnlyNames'))
    })
    expect(within(getDropdownContent()).getByText('Wing')).toBeInTheDocument()
    expect(within(getDropdownContent()).getByText('Tower')).toBeInTheDocument()
    act(() => {
      closeDropdown(screen.getByLabelText('withOnlyNames'))
    })

    act(() => {
      openDropdown(screen.getByLabelText('withStringDisplayNames'))
    })
    expect(within(getDropdownContent()).getByText('Up')).toBeInTheDocument()
    expect(within(getDropdownContent()).getByText('Down')).toBeInTheDocument()
    act(() => {
      closeDropdown(screen.getByLabelText('withStringDisplayNames'))
    })

    act(() => {
      openDropdown(screen.getByLabelText('withDictionaryDisplayNames'))
    })
    expect(
      within(getDropdownContent()).getByText('undefined')
    ).toBeInTheDocument()
    expect(within(getDropdownContent()).getByText('analog')).toBeInTheDocument()
  })

  test('Enum value read only display', async () => {
    // Test that we get an enum option's display name in read only mode.
    await setup({
      twin: {
        etag: makeTwinEtag(),
        siteID: '123',
        nominalCoolingCapacityUnit: 'joulePerHour',
        metadata: {
          modelId: 'my-model',
        },
      },
      model: modelFromSchemas({
        siteID: 'string',
        nominalCoolingCapacityUnit: {
          '@type': 'Enum',
          enumValues: [
            {
              enumValue: 'joulePerHour',
              name: 'joulePerHour',
              displayName: 'J/h',
            },
          ],
          valueSchema: 'string',
        },
      }),
    })

    expect(screen.getByText('J/h')).toBeInTheDocument()
  })

  test('Read-only properties', async () => {
    await setup({
      restrictedFields: {
        hiddenFields: [],
        readOnlyFields: ['/updateThis', '/group/hello'],
      },
    })

    act(() => {
      userEvent.click(screen.getByTitle('Edit twin information'))
    })

    act(() => {
      const updateThisTextBox = screen.getByLabelText('Update this')
      userEvent.type(updateThisTextBox, 'this will never happen')
    })

    act(() => {
      const helloTextBox = screen.getByLabelText('Hello')
      userEvent.type(helloTextBox, 'nor will this')
    })

    expect(screen.getByLabelText('Update this')).toHaveValue('update this')
    expect(screen.getByLabelText('Hello')).toHaveValue('string in group')
  })

  test('No Show More button if there is no more to show', async () => {
    await setup({
      twin: {
        ...testTwin,
        emptyValue: 'not empty this time',
        nullValue: 'actually not null this time',
      },
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    expect(screen.queryByText('Show more')).toBeNull()
  })

  test('Revert to "Show more" if we saved and there are still missing fields', async () => {
    await setup()

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    await save()

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    expect(screen.queryByText('Show more')).not.toBeNull()
  })

  test("Don't show edit button if we can't edit", async () => {
    await setup({
      twin: {
        ...testTwin,
        permissions: {
          edit: false,
        },
      },
      waitForEditButton: false,
    })

    await waitFor(() =>
      expect(screen.getByText('plainText.information')).toBeInTheDocument()
    )
    expect(screen.queryByTitle('Edit twin information')).toBeNull()
  })

  test('Unnamed twin', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
      },
    })

    expect(screen.getAllByText('Unnamed twin')[0]).toBeInTheDocument()
  })

  test('404 twin', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        type: 'error',
        statusCode: 404,
      },
      waitForEditButton: false,
    })

    await waitFor(() =>
      expect(screen.getByText('plainText.notFindTwin')).toBeInTheDocument()
    )
  })

  test('500 twin', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        type: 'error',
        statusCode: 500,
      },
      waitForEditButton: false,
    })

    await waitFor(() =>
      expect(screen.getByText('plainText.errorLoadingTwin')).toBeInTheDocument()
    )
  })

  test('403 twin', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        type: 'error',
        statusCode: 403,
      },
      waitForEditButton: false,
    })

    await waitFor(() =>
      expect(
        screen.getByText('plainText.insufficientPrivilegesForTwin')
      ).toBeInTheDocument()
    )
  })

  test('Twin with rogue fields', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        rogue: 'field',
      },
      waitForEditButton: false,
    })

    await waitFor(() =>
      expect(
        screen.getByText('plainText.twinWithFieldsNotInModel')
      ).toBeInTheDocument()
    )
  })

  test('Twin with an Array field', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        mappedIds: [
          {
            exactType: 'PostalAddressIdentity',
            scope: 'ORG',
            scopeId: '123',
            value: 'Some address',
          },
        ],
      },
      model: {
        ...testModel,
        contents: [
          ...testModel.contents,
          makeProperty(
            'mappedIds',
            {
              '@type': 'Array',
              elementSchema: 'dtmi:com:willowinc:SpaceMappedIdObject;1',
            },
            {
              displayName: 'Mapped IDs',
            }
          ),
        ],
      },
    })

    expect(screen.queryByText('Mapped IDs')).toBeInTheDocument()
  })

  test('Twin with warranty: customProperties containing only Warranty', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        customProperties: {
          Warranty: {
            startDate: '01/01/2022',
            endDate: '01/01/2025',
            provider: 'Willow',
          },
        },
      },
      model: testModelWithCustomProperties,
    })

    // Custom Properties section should be hidden
    expect(screen.queryByText('Custom Properties')).not.toBeInTheDocument()
    expect(screen.queryByText('Warranty')).not.toBeInTheDocument()

    // Check if warranty section is being displayed
    expect(screen.getByText('plainText.warranty')).toBeInTheDocument()

    // Check if warranty info is being displayed
    expect(screen.getByText('labels.startDate')).toBeInTheDocument()
    expect(screen.getByText('labels.endDate')).toBeInTheDocument()
    expect(screen.getByText('plainText.warrantyProvider')).toBeInTheDocument()
  })

  test('Twin with warranty: customProperties containing Warranty and other keys', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        customProperties: {
          Warranty: {
            startDate: '01/01/2022',
            endDate: '01/01/2025',
            provider: 'Willow',
          },
          otherKeys: { data: 'data' },
        },
      },
      model: testModelWithCustomProperties,
    })

    // Custom Properties section should be displayed, but Warranty is hidden
    expect(screen.queryByText('Custom Properties')).toBeInTheDocument()
    expect(screen.queryByText('Warranty')).not.toBeInTheDocument()
    expect(screen.queryByText('otherKeys')).toBeInTheDocument()

    // Check if warranty section is being displayed
    expect(screen.getByText('plainText.warranty')).toBeInTheDocument()

    // Check if warranty info is being displayed
    expect(screen.getByText('labels.startDate')).toBeInTheDocument()
    expect(screen.getByText('labels.endDate')).toBeInTheDocument()
    expect(screen.getByText('plainText.warrantyProvider')).toBeInTheDocument()
  })

  test('Custom properties should be displayed as plain text with key/value pairs on separate lines', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        customProperties: {
          propertyOne: {
            Status: 'Installed',
            Class: 'HVAC',
            OutofService: 'NO',
          },
          propertyTwo: {
            UnitType: 'CV',
            UnitNumber: '123',
          },
        },
      },
      model: testModelWithCustomProperties,
    })

    // Custom Properties section should be displayed
    expect(screen.queryByText('Custom Properties')).toBeInTheDocument()
    expect(screen.queryByText('propertyOne')).toBeInTheDocument()
    expect(screen.queryByText('propertyTwo')).toBeInTheDocument()

    // Both properties should be formatted with their values on separate lines
    expect(
      screen.getByText(/Status: Installed\nClass: HVAC\nOutofService: NO/, {
        collapseWhitespace: false,
      })
    ).toBeInTheDocument()

    expect(
      screen.getByText(/UnitType: CV\nUnitNumber: 123/, {
        collapseWhitespace: false,
      })
    ).toBeInTheDocument()

    // Check the properties are shown as text areas in edit mode
    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    expect(
      screen.getByText(/Status: Installed\nClass: HVAC\nOutofService: NO/, {
        collapseWhitespace: false,
      }).nodeName
    ).toBe('TEXTAREA')

    expect(
      screen.getByText(/UnitType: CV\nUnitNumber: 123/, {
        collapseWhitespace: false,
      }).nodeName
    ).toBe('TEXTAREA')
  })

  test('Custom properties should gracefully handle unexpected values', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        customProperties: {
          propertyOne: 'Not a map',
        },
      },
      model: testModelWithCustomProperties,
    })

    // Custom Properties section should be displayed
    expect(screen.queryByText('Custom Properties')).toBeInTheDocument()
    expect(screen.queryByText('propertyOne')).toBeInTheDocument()

    // Per the schema it shouldn't be valid for the value to be anything but a map,
    // but nevertheless in case it happens it should be handled gracefully, so the value
    // is simply returned as a plain string.
    expect(screen.getByText('Not a map')).toBeInTheDocument()

    // Check the property is shown as a text field in edit mode
    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    expect(screen.getByDisplayValue('Not a map').nodeName).toBe('INPUT')
  })

  test('Group property with no fields should not be displayed', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        myGroup: {
          x: '',
          $metadata: {
            field: 'val',
          },
        },
      },
      model: {
        ...testModel,
        contents: [
          ...testModel.contents,
          makeProperty(
            'myGroup',
            {
              '@type': 'Map',
              mapKey: {
                name: 'key',
                schema: 'string',
              },
              mapValue: {
                name: 'key',
                schema: 'string',
              },
            },
            {
              displayName: 'My Group',
            }
          ),
        ],
      },
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    expect(screen.queryByText('My Group')).not.toBeInTheDocument()
  })

  test('Group property with only empty string fields should not be displayed', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        myGroup: {
          x: '',
          y: '',
          $metadata: {
            field: 'val',
          },
        },
      },
      model: {
        ...testModel,
        contents: [
          ...testModel.contents,
          makeProperty(
            'myGroup',
            {
              '@type': 'Map',
              mapKey: {
                name: 'key',
                schema: 'string',
              },
              mapValue: {
                name: 'key',
                schema: 'string',
              },
            },
            {
              displayName: 'My Group',
            }
          ),
        ],
      },
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    expect(screen.queryByText('My Group')).not.toBeInTheDocument()
  })

  test('Component properties are read only', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        fan: {
          someFanField: 'seven',
        },
      },
      model: testModelWithComponent,
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    expect(screen.getByLabelText('someFanField')).toHaveAttribute('readonly')
  })

  test('Empty components do not appear at all', async () => {
    await setup({
      twin: {
        id: '123',
        siteID: '123',
        metadata: {
          modelId: 'myModelId',
        },
        fan: {
          $metadata: {
            // Just put something in here to make sure that having a $metadata
            // value doesn't stop a Component from being identified as empty.
            someStuff: 123,
          },
        },
      },
      model: testModelWithComponent,
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    act(() => {
      fireEvent.click(screen.getByText('Show more'))
    })

    expect(screen.queryByText('fan')).not.toBeInTheDocument()
  })

  test('Validation', async () => {
    // Test that the different kinds of invalid inputs in numeric fields
    // produce appropriate error messages and prevent saving.
    const { submitRequests } = await setup({
      twin: {
        etag: makeTwinEtag(),
        siteID: '123',
        id: '123',
        metadata: {
          modelId: 'my-model',
        },
      },
      model: modelFromSchemas({
        siteID: 'string',
        integer: 'integer',
        long: 'long',
        float: 'float',
        double: 'double',
        duration: 'duration',
        object: {
          '@type': 'Object',
          fields: [
            {
              name: 'nestedNumber',
              schema: 'double',
            },
          ],
        },
      }),
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    act(() => {
      fireEvent.click(screen.getByText('Show more'))
    })

    const integer = screen.getByLabelText('integer')
    const long = screen.getByLabelText('long')
    const float = screen.getByLabelText('float')
    const double = screen.getByLabelText('double')
    const nestedNumber = screen.getByLabelText('nestedNumber')

    userEvent.type(integer, '2.9')
    userEvent.type(long, (2 ** 55).toString())
    userEvent.type(float, 'fish')
    userEvent.type(double, 'nine point nine')
    userEvent.type(nestedNumber, 'also not a number')

    act(() => {
      openDropdown(screen.getByLabelText('duration'))
    })

    const durationSeconds = screen.getByLabelText('plainText.seconds')
    act(() => {
      userEvent.clear(durationSeconds)
      userEvent.type(durationSeconds, 'false')
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Save'))
    })

    await waitFor(() =>
      expect(screen.getByText('Must be an integer')).toBeInTheDocument()
    )
    expect(screen.queryAllByText('Must be a number')).toHaveLength(3)
    expect(screen.getByText('Number is out of range')).toBeInTheDocument()
    expect(
      screen.getByText('plainText.mustBeValidDuration')
    ).toBeInTheDocument()
    expect(submitRequests).toHaveLength(0)

    userEvent.clear(integer)
    userEvent.type(integer, '2')
    userEvent.clear(long)
    userEvent.type(long, '22')
    userEvent.clear(float)
    userEvent.type(float, '2.2')
    userEvent.clear(double)
    userEvent.type(double, '2e8') // Exponential format is allowed
    userEvent.clear(nestedNumber)
    userEvent.type(nestedNumber, '3')

    userEvent.clear(durationSeconds)
    userEvent.type(durationSeconds, '30')

    await save()

    // Now that we entered valid values into the fields, the save should go
    // through.
    await waitFor(() => expect(submitRequests).toHaveLength(1))
  })

  test('Sneaky zero values do not break duration', async () => {
    // Make sure that values like "-0" or "00" don't trick the duration field
    // into sending invalid duration strings.
    const { submitRequests } = await setup({
      twin: {
        etag: makeTwinEtag(),
        siteID: '123',
        id: '123',
        metadata: {
          modelId: 'my-model',
        },
      },
      model: modelFromSchemas({
        siteID: 'string',
        duration: 'duration',
      }),
    })

    act(() => {
      fireEvent.click(screen.getByTitle('Edit twin information'))
    })

    act(() => {
      fireEvent.click(screen.getByText('Show more'))
    })

    act(() => {
      openDropdown(screen.getByLabelText('duration'))
    })

    const durationMonths = screen.getByLabelText('plainText.months')
    act(() => {
      userEvent.clear(durationMonths)
      userEvent.type(durationMonths, '-0')
    })

    const durationDays = screen.getByLabelText('plainText.days')
    act(() => {
      userEvent.clear(durationDays)
      userEvent.type(durationDays, '00')
    })

    const durationHours = screen.getByLabelText('plainText.hours')
    act(() => {
      userEvent.clear(durationHours)
      userEvent.type(durationHours, '7')
    })

    await save()

    await waitFor(() => expect(submitRequests).toHaveLength(1))
    expect(submitRequests[0].body[0].value).toBe('PT7H')
  })
  test('Version History', async () => {
    await setup()

    // Open version history dropdown
    act(() => {
      openDropdown(screen.getByTestId('versionHistoryDropdown'))
    })

    let versionHistoryOptions = screen.getAllByRole('option')

    expect(versionHistoryOptions.length).toBe(5)

    // One of the version's timestamp should be current datetime
    // Check timestamp formating, eg TODAY HH:MM
    expect(screen.getByText(/plainText.today/)).toBeInTheDocument()

    // Select 4th version option
    act(() => {
      fireEvent.click(versionHistoryOptions[3])
    })

    // Check if viewing old version message component is displayed
    expect(screen.getByText(/plainText.currentVersion/)).toBeInTheDocument()

    // versionHistoryOptions[3] edited updateThis field to "new value"
    expect(screen.getByText(/new value/)).toBeInTheDocument()

    // Select 3rd version option
    act(() => {
      openDropdown(screen.getByTestId('versionHistoryDropdown'))
    })

    versionHistoryOptions = screen.getAllByRole('option')

    act(() => {
      fireEvent.click(versionHistoryOptions[2])
    })

    // versionHistoryOptions[2] edited the nested field group.hello to "new nested value"
    expect(screen.getByText(/new nested value/)).toBeInTheDocument()

    // Select 2nd version option
    act(() => {
      openDropdown(screen.getByTestId('versionHistoryDropdown'))
    })

    versionHistoryOptions = screen.getAllByRole('option')

    act(() => {
      fireEvent.click(versionHistoryOptions[1])
    })

    // versionHistoryOptions[1] edited the field name to "new value"
    // There should be 2 text with "new value"
    expect(screen.getAllByText(/new value/).length).toBe(2)

    // Select 1st version option
    act(() => {
      openDropdown(screen.getByTestId('versionHistoryDropdown'))
    })

    versionHistoryOptions = screen.getAllByRole('option')

    act(() => {
      fireEvent.click(versionHistoryOptions[0])
    })

    // versionHistoryOptions[0] deleted the field updateThis
    expect(screen.getAllByText(/new value/)[1]).toHaveStyle(
      'text-decoration: line-through;'
    )
  })
})
