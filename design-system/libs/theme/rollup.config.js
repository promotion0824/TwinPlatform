// this config doesn't change anything, but it's required to
// include woff2 file that is imported from material-symbols/sharp.css
// into the bundle. Otherwise, the consumer needs to configure webpack
// to handle woff2 files.
// BUG 81807
// https://dev.azure.com/willowdev/Unified/_workitems/edit/81807
module.exports = (config) => {
  return config
}
