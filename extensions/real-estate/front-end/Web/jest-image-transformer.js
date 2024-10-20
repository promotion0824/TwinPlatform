// When we `import` an image file, the webpack `file-loader` plugin generates a
// URL for it and makes that the default export of the "module". For tests we
// we're unlikely to need this; returning the filename should get us by.

module.exports = {
  process(src, filePath) {
    return { code: `module.exports = ${JSON.stringify(filePath)};` }
  },
}
