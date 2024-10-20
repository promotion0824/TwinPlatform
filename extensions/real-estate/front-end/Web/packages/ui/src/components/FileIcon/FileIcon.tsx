import * as icons from './icons'

/**
 * We have a set of icons for particular file types. Our notion of a file type
 * is based on the file extension, though it's not a one-to-one mapping; eg.
 * ".doc" and ".docx" have the same icon. If a file does not have a recognised
 * extension, it is given a generic icon which just says "file".
 */

export const fileExtensionMap = {
  '.txt': 'txt',
  '.csv': 'csv',
  '.pdf': 'pdf',
  '.zip': 'zip',
  '.jpg': 'jpg',
  '.png': 'png',
  '.dwg': 'dwg',
  '.doc': 'doc',
  '.docx': 'doc',
  '.xls': 'xls',
  '.xlsx': 'xls',
  '.ppt': 'ppt',
  '.pptx': 'ppt',
}

const fileColors = {
  csv: '#47ce4a',
  txt: '#9cccef',
  pdf: '#fc2d3b',
  zip: '#ff8400',
  jpg: '#c36dc5',
  png: '#c36dc5',
  dwg: '#ffd653',
  doc: '#63a9e3',
  xls: '#70da72',
  ppt: '#fd6c76',
}

// These are used elsewhere and should be updated when the SVGs change
export const getFileTypeColor = (fileType: string | undefined) => {
  if (fileType != null) {
    return fileColors[fileType] ?? '#d1d1d1'
  } else {
    return '#d1d1d1'
  }
}

export default function FileIcon({ filename }: { filename: string }) {
  const IconComponent = icons[getFileType(filename)]
  return <IconComponent />
}

function getFileType(filename: string) {
  const ext = getExtension(filename).toLowerCase()
  return fileExtensionMap[ext] ?? 'generic'
}

export function getExtension(filename: string) {
  return filename.match(/\.[^.]*$/)?.[0] ?? ''
}
