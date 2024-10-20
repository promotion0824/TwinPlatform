const postcssMobileHover = require('postcss-mobile-hover') // eslint-disable-line
const postcssMomentumScrolling = require('postcss-momentum-scrolling') // eslint-disable-line
const postcssPresetEnv = require('postcss-preset-env') // eslint-disable-line
const tailwindcss = require('tailwindcss') // eslint-disable-line
const autoprefixer = require('autoprefixer') // eslint-disable-line

const isMobile = process.env.IS_MOBILE === 'true'

module.exports = {
  plugins: [
    postcssPresetEnv({
      stage: false,
      features: {
        'custom-media-queries': true,
        'custom-selectors': true,
        'media-query-ranges': true,
      },
      importFrom: isMobile
        ? './packages/mobile/ui/src/theme.css'
        : './packages/ui/src/theme.css',
    }),
    postcssMobileHover(),
    postcssMomentumScrolling(),
    tailwindcss,
    autoprefixer,
  ],
}
