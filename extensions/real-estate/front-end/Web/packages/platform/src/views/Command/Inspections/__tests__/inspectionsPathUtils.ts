import getInspectionsPath from '../getInspectionsPath'
import makeScopedInspectionsPath from '../makeScopedInspectionsPath'

describe('tests utils for generating inspections path', () => {
  describe('view without scopeId nor siteId', () => {
    test('Inspections page', () => {
      expect(getInspectionsPath()).toEqual('/inspections')
      expect(makeScopedInspectionsPath()).toEqual('/inspections')
    })

    test('Inspections page - selected zone', () => {
      const params = {
        pageName: 'zones' as const,
        pageItemId: 'zone123',
      }
      expect(getInspectionsPath(undefined, params)).toEqual(
        '/inspections/zones/zone123'
      )
      expect(makeScopedInspectionsPath(undefined, params)).toEqual(
        '/inspections/zones/zone123'
      )
    })

    test('Inspections page - selected check', () => {
      const params = {
        pageName: 'checks' as const,
        pageItemId: 'check123',
        inspectionId: 'inspectionXYZ',
      }
      expect(getInspectionsPath(undefined, params)).toEqual(
        '/inspections/inspectionXYZ/checks/check123'
      )
      expect(
        makeScopedInspectionsPath(undefined, { ...params, pageName: 'check' })
      ).toEqual('/inspections/inspectionXYZ/check/check123')
    })
  })

  // TODO: remove this test once scope selector feature is complete.
  describe('Single site view', () => {
    test('Inspections page', () => {
      expect(getInspectionsPath('12345')).toEqual('/sites/12345/inspections')
    })

    test('Inspections page - Usage tab', () => {
      expect(getInspectionsPath('12345', { pageName: 'usage' })).toEqual(
        '/sites/12345/inspections/usage'
      )
    })

    test('Inspections page - Zones tab', () => {
      expect(getInspectionsPath('12345', { pageName: 'zones' })).toEqual(
        '/sites/12345/inspections/zones'
      )
    })

    test('Inspections page - selected zone', () => {
      expect(
        getInspectionsPath('12345', {
          pageName: 'zones',
          pageItemId: 'zone123',
        })
      ).toEqual('/sites/12345/inspections/zones/zone123')
    })

    test('Inspections page - selected check', () => {
      expect(
        getInspectionsPath('12345', {
          pageName: 'checks',
          pageItemId: 'check123',
          inspectionId: 'inspectionXYZ',
        })
      ).toEqual('/sites/12345/inspections/inspectionXYZ/checks/check123')
    })
  })

  describe('scoped view', () => {
    const scopeId = 'scope-123'
    test('Inspections page', () => {
      expect(makeScopedInspectionsPath(scopeId)).toEqual(
        `/inspections/scope/${scopeId}`
      )
    })

    test('Inspections page - Usage tab', () => {
      expect(makeScopedInspectionsPath(scopeId, { pageName: 'usage' })).toEqual(
        `/inspections/scope/${scopeId}/usage`
      )
    })

    test('Inspections page - Zones tab', () => {
      expect(makeScopedInspectionsPath(scopeId, { pageName: 'zones' })).toEqual(
        `/inspections/scope/${scopeId}/zones`
      )
    })

    test('Inspections page - selected zone', () => {
      expect(
        makeScopedInspectionsPath(scopeId, {
          pageName: 'zones',
          pageItemId: 'zone123',
        })
      ).toEqual(`/inspections/scope/${scopeId}/zones/zone123`)
    })

    test('Inspections page - selected check', () => {
      expect(
        makeScopedInspectionsPath(scopeId, {
          pageName: 'check',
          pageItemId: 'check123',
          inspectionId: 'inspection/inspectionXYZ',
        })
      ).toEqual(
        `/inspections/scope/${scopeId}/inspection/inspectionXYZ/check/check123`
      )
    })
  })
})
