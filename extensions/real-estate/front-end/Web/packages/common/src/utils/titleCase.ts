import titleCaseFrench from 'titlecase-french'
import toEnglishTitleCase from 'titlecase'

// https://www.npmjs.com/package/titlecase-french#keepcapitalizedspecials
const defaultCapitalizedSpecials = 'À,Ç,É'
titleCaseFrench.keepCapitalizedSpecials(defaultCapitalizedSpecials)

// please refer to tests for examples
const titleCase = ({ text, language }: { text: string; language: string }) => {
  if (language === 'fr') {
    return titleCaseFrench.convert(text)
  }
  return toEnglishTitleCase(text.toLowerCase())
}

export default titleCase
