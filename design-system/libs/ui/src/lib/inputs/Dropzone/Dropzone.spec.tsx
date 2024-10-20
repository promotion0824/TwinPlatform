import dropzoneDescription from './dropzoneDescription'

describe('dropzoneDescription', () => {
  it('should return an empty string if no options are provided', () => {
    expect(dropzoneDescription({})).toBe('')
  })

  it('should return maximum file info if maxFiles is provided', () => {
    expect(dropzoneDescription({ maxFiles: 1 })).toBe('Maximum 1 file.')
    expect(dropzoneDescription({ maxFiles: 2 })).toBe('Maximum 2 files.')
  })

  it('should return accepted files info if acceptedFiles is provided', () => {
    expect(dropzoneDescription({ acceptedFiles: ['jpeg', 'png'] })).toBe(
      'Accepts: jpeg, png.'
    )
  })

  it('should return max file size info if maxSize is provided', () => {
    expect(dropzoneDescription({ maxSize: 500 })).toBe('Max file size: 500 B.')
    expect(dropzoneDescription({ maxSize: 1024 })).toBe('Max file size: 1 KB.')
    expect(dropzoneDescription({ maxSize: 1024 * 1024 })).toBe(
      'Max file size: 1 MB.'
    )
    expect(dropzoneDescription({ maxSize: 2.5678 * 1024 * 1024 })).toBe(
      'Max file size: 2.57 MB.'
    )
  })

  it('should return a string with all info when multiple options are provided', () => {
    expect(
      dropzoneDescription({
        acceptedFiles: ['jpeg', 'png'],
        maxFiles: 2,
        maxSize: 1024 * 1024,
      })
    ).toBe('Maximum 2 files. Accepts: jpeg, png. Max file size: 1 MB.')
  })
})
