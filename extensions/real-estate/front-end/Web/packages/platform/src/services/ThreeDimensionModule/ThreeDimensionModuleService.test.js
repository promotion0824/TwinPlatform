import axios from 'axios'
import {
  post3dModule,
  get3dModuleFile,
  get3dModule,
  delete3dModule,
} from './ThreeDimensionModuleService'
import * as AutoDeskService from '../AutoDesk/AutoDeskService'

const moduleResponseData = {
  id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  name: 'abc.nwd',
  visualId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  url: 'ADSJFNuuaebEsbKKDJ',
  sortOrder: 0,
  canBeDeleted: true,
  typeName: 'string',
  groupType: 'string',
  moduleTypeId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  moduleGroup: {
    id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    name: 'string',
    sortOrder: 0,
    siteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  },
}

const expectedGetData = {
  id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  name: 'abc.nwd',
  visualId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  url: 'ADSJFNuuaebEsbKKDJ',
  sortOrder: 0,
  canBeDeleted: true,
  typeName: 'string',
  groupType: 'string',
  moduleTypeId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  moduleGroup: {
    id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    name: 'string',
    sortOrder: 0,
    siteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
  },
}
const ERROR_MESSAGE = 'fetch error'

describe('ThreeDimensionModuleService', () => {
  describe('post3dModule', () => {
    test('should return expected data', async () => {
      jest.spyOn(axios, 'post').mockResolvedValue({ data: moduleResponseData })

      const response = await post3dModule({}, {})

      expect(response).toMatchObject(moduleResponseData)
    })

    test('should return error when exception error happens', async () => {
      jest.spyOn(axios, 'post').mockRejectedValue(new Error(ERROR_MESSAGE))

      await expect(post3dModule({}, {})).rejects.toThrowError(ERROR_MESSAGE)
    })
  })

  describe('get3dModule', () => {
    test('should return expected data', async () => {
      jest.spyOn(axios, 'get').mockResolvedValue({ data: moduleResponseData })

      const response = await get3dModule({}, {})

      expect(response).toMatchObject(expectedGetData)
    })

    test('should return error when exception error happens', async () => {
      jest.spyOn(axios, 'get').mockRejectedValue(new Error(ERROR_MESSAGE))

      await expect(get3dModule({}, {})).rejects.toThrowError(ERROR_MESSAGE)
    })
  })

  describe('delete3dModule', () => {
    test('should return expected data', async () => {
      jest.spyOn(axios, 'delete').mockResolvedValue({ data: { status: 204 } })

      const response = await delete3dModule('siteId')

      expect(response).toBe(204)
    })

    test('should return error when exception error happens', async () => {
      jest.spyOn(axios, 'delete').mockRejectedValue(new Error(ERROR_MESSAGE))

      await expect(delete3dModule({}, {})).rejects.toThrowError(ERROR_MESSAGE)
    })
  })

  describe('get3dModuleFile', () => {
    const fileName = 'abc.nwd'
    const blobUrn = 'dXJuOmFkc2sub2JqZWN0c'
    jest.spyOn(AutoDeskService, 'getAutoDeskAccessToken').mockResolvedValue({
      access_token: 'abc',
      token_type: 'Bearer',
    })

    test('should return expected blob', async () => {
      const blob = new Blob(['blob'])
      jest
        .spyOn(AutoDeskService, 'getAutoDeskModuleFile')
        .mockResolvedValue(blob)
      jest.spyOn(AutoDeskService, 'getAutoDeskFileManifest').mockResolvedValue({
        fileInfo: {
          urn: 'asdf',
        },
        progress: 'complete',
      })

      const response = await get3dModuleFile(blobUrn, fileName)

      expect(response instanceof File).toBeTruthy()
      expect(response.name).toBe(fileName)
    })

    test('should return error when exception error happens', async () => {
      jest
        .spyOn(AutoDeskService, 'getAutoDeskModuleFile')
        .mockRejectedValue(new Error(ERROR_MESSAGE))

      await expect(get3dModuleFile(blobUrn, fileName)).rejects.toThrowError(
        ERROR_MESSAGE
      )
    })

    test('should return error when Autodesk is still uploading a file', async () => {
      const progress = '0% complete'
      jest
        .spyOn(AutoDeskService, 'getAutoDeskFileManifest')
        .mockResolvedValue({ progress })

      await expect(get3dModuleFile(blobUrn, fileName)).rejects.toThrowError(
        `Autodesk is still processing. ${progress}`
      )
    })
  })
})
