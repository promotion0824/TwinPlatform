// Why? We used to use jest-svg-transformer but that is abandoned and does not
// work with jest >= 27. svg-jest is another similar package that does work
// with jest >= 27 but imports svg files as plain objects, not React components, so it is not
// suitable for us. So instead we copied this out of this github issue:
// https://github.com/cwmoo740/jest-svg-transformer/issues/3#issuecomment-911774849
const path = require('path')

module.exports = {
  process(src, filePath) {
    if (path.extname(filePath) !== '.svg') {
      return src
    }

    const name = `svg-${path.basename(filePath, '.svg')}`
      .split(/\W+/)
      .map((x) => `${x.charAt(0).toUpperCase()}${x.slice(1)}`)
      .join('')

    return {
      code: `
const React = require('react');
function ${name}(props) {
  return React.createElement(
    'svg',
    Object.assign({}, props, {'data-file-name': ${name}.name})
  );
}
module.exports = ${name};
            `,
    }
  },
}
