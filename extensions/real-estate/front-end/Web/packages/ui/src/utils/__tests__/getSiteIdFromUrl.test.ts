import getSiteIdFromUrl from '../getSiteIdFromUrl'

describe('getSiteIdFromUrl', () => {
  test('should return siteId when siteId is matched as part of url but not as query parameter', async () => {
    const url = '/sites/a6b78f54-9875-47bc-9612-aa991cc464f3'

    expect(getSiteIdFromUrl(url)).toBe('a6b78f54-9875-47bc-9612-aa991cc464f3')
  })

  test('should return nothing when siteId is not matched as part of url', async () => {
    const urlHasNoSiteId = '/admin'

    expect(getSiteIdFromUrl(urlHasNoSiteId)).not.toBeDefined()

    const urlHasPortfolioId =
      '/admin/portfolios/4941aeb2-8c4b-4e3c-8881-a3c6fb4cd112'

    expect(getSiteIdFromUrl(urlHasPortfolioId)).not.toBeDefined()

    const urlWithSiteIdAsQueryParams =
      '/admin/users?siteId=e719ac18-192b-4174-91db-b3a624f1f1a4'

    expect(getSiteIdFromUrl(urlWithSiteIdAsQueryParams)).not.toBeDefined()
  })
})
