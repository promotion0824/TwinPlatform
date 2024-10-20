import themes from './lib/themes'

export type ThemeName = keyof typeof themes

const getTheme = (themeName: ThemeName = 'dark') => themes[themeName]

export default getTheme
