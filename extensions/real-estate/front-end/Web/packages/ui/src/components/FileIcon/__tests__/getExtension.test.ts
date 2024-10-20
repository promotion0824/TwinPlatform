import { getExtension } from '../FileIcon'

test('getExtension', () => {
  expect(getExtension('hello.pdf')).toEqual('.pdf')
  expect(getExtension('myfile.xml.zip')).toEqual('.zip')
  expect(getExtension('.htaccess')).toEqual('.htaccess')
  expect(getExtension('makefile')).toEqual('')
})
