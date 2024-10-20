import { rest } from 'msw'

export async function proxyJson(req, res, ctx) {
  const response = await ctx.fetch(req)
  return res(ctx.status(response.status), ctx.json(await response.json()))
}

/**
 * Takes a handler (as produced by the `rest` function from msw) and returns a
 * new handler which first tries the function from the first handler, and
 * returns its response if it's not `undefined`, else passes through the
 * request to the real server.
 */
export function wrapHandlerWithFallback(handler) {
  return rest[handler.info.method.toLowerCase()](
    handler.info.path,
    (req, res, ctx) => {
      const response = handler.resolver(req, res, ctx)
      if (response !== undefined) {
        return response
      }
      return proxyJson(req, res, ctx)
    }
  )
}

/**
 * Takes a handler and returns a new one with the ":/region" prefix stripped
 * from the path, if it exists. Useful for tests because they do not know about
 * these prefix, though it would be better in future to eliminate the
 * inconsistency rather than working around it.
 */
export function withoutRegion(handler) {
  return rest[handler.info.method.toLowerCase()](
    handler.info.path.replace(new RegExp('^/:region'), ''),
    handler.resolver
  )
}

export function csvify(items) {
  return items.map((r) => r.map(csvifyCell).join(',')).join('\n')
}

function csvifyCell(cell) {
  return `"${(cell ?? '').toString().replaceAll('"', '""')}"`
}
