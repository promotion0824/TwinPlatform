import axios from 'axios'
import {
  getAutoDeskModuleFile,
  getAutoDeskFileManifest,
  getAutoDeskAccessToken,
} from './AutoDeskService'

const ERROR_MESSAGE = 'fetch error'

const URN = 'ASDFJIUuue=='
const AUTHORIZATION = 'Bearer #Ijvfvirj#234'

describe('AutoDesk Service (3rd party APIs)', () => {
  describe('AutoDesk oauth token', () => {
    test('should return expected data', async () => {
      const responseData = {
        access_token: 'asdf',
        token_type: 'Bearer',
      }
      jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

      const response = await getAutoDeskAccessToken()

      expect(response).toMatchObject(responseData)
    })

    test('should return error when exception error happens', async () => {
      jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

      await expect(getAutoDeskAccessToken()).rejects.toThrowError(ERROR_MESSAGE)
    })
  })
  describe('AutoDesk 3rd party APIs', () => {
    describe('get file URN from AutoDesk manifest', () => {
      test('should return existing file data when Autodesk competed uploading', async () => {
        const graphicData = {
          urn: 'urn:adsk.viewing:fs.file:dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtN2NiZjBiOGMtN2NhMy00YTk5LWFlMjMtOGVlYmQ5OGQxNmYxLWRldi9BcmNoaXRlY3R1cmVfMjAyMTEyMjEyMjU1MTgubndk/output/0/0.svf',
          role: 'graphics',
          mime: 'application/autodesk-svf',
          guid: 'a59d92c7-f8e6-4295-b263-7451b762eab5',
          type: 'resource',
        }
        const responseData = {
          derivatives: [
            {
              children: [
                {
                  children: [graphicData],
                },
              ],
            },
          ],
          progress: undefined,
        }
        jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

        const response = await getAutoDeskFileManifest(URN, AUTHORIZATION)

        expect(response).toStrictEqual({
          fileInfo: graphicData,
          progress: undefined,
        })
      })
      test('should return incompleted progress data when Autodesk is still uploading', async () => {
        const progress = '0% complete'
        const responseData = {
          derivatives: [],
          progress,
        }
        jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

        const response = await getAutoDeskFileManifest(URN, AUTHORIZATION)

        expect(response).toStrictEqual({
          fileInfo: undefined,
          progress,
        })
      })

      test('should return error when exception error happens', async () => {
        jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

        await expect(
          getAutoDeskFileManifest(URN, AUTHORIZATION)
        ).rejects.toThrowError(ERROR_MESSAGE)
      })
    })

    describe('download the source file', () => {
      const DERIVATIVE_URN = 'urn:ajsc'
      test('should return stream data', async () => {
        const type = 'application/octet-stream'
        const str = 'test'
        const responseData = new Blob([str], {
          type,
        })
        jest.spyOn(axios, 'get').mockResolvedValue({ data: responseData })

        const response = await getAutoDeskModuleFile(
          URN,
          DERIVATIVE_URN,
          AUTHORIZATION
        )

        expect(response instanceof Blob).toBeTruthy()
        expect(response.size).toBe(str.length)
        expect(response.type).toBe(type)
      })

      test('should return error when exception error happens', async () => {
        jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

        await expect(
          getAutoDeskModuleFile(URN, DERIVATIVE_URN, AUTHORIZATION)
        ).rejects.toThrowError(ERROR_MESSAGE)
      })
    })
  })
})
