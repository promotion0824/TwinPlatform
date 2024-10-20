const getSiteIdFromUrl = (url: string): string | undefined =>
  url.match(/\/sites\/(.+?)(\/|$)/)?.[1]

export default getSiteIdFromUrl
