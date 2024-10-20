const specialCharacters = '@#$%^&*-_!+=[]{}|\\:\',.?/`~"();'
const specialCharactersRegex = specialCharacters
  .replace('\\', '\\\\', /g/)
  .replace('[', '\\[', /g/)
  .replace(']', '\\]', /g/)
  .replace('(', '\\(', /g/)
  .replace(')', '\\)', /g/)
  .replace('{', '\\{', /g/)
  .replace('}', '\\}', /g/)
  .replace('-', '\\-', /g/)

export default [
  {
    description: 'minimum 8 characters',
    isValid: (value) => value.length >= 8,
  },
  {
    description: 'at least 1 upper case character A-Z',
    isValid: (value) => /[A-Z]/.test(value),
  },
  {
    description: 'at least 1 lower case character a-z',
    isValid: (value) => /[a-z]/.test(value),
  },
  {
    description: 'at least 1 number 0-9',
    isValid: (value) => /[0-9]/.test(value),
  },
  {
    description: 'at least 1 special character',
    isValid: (value) => new RegExp(`[${specialCharactersRegex}]`).test(value),
    title: specialCharacters.split('').join(' '),
  },
  {
    description: 'no spaces',
    isValid: (value) => !/ /.test(value),
  },
]
