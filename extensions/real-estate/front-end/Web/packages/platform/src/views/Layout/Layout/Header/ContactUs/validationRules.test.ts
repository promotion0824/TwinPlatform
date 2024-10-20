import { isValidEmail } from './validationRules'

describe('email address validation', () => {
  it('should validate all valid email address', () => {
    const validEmails = [
      'user@example.com',
      'john.doe@example.co.in',
      'jane_doe123@example.travel',
      'first.last@subdomain.example',
    ]

    validEmails.forEach((email) => {
      expect(isValidEmail(email)).toBe(true)
    })
  })

  it('should reject all invalid email address', () => {
    const invalidEmails = [
      'invalid.email',
      'missing@atdotcom',
      '@missingusername.com',
      'user@.missingtld',
      'user@missingtld.',
      'user@example..com',
      'user@example.com.',
      'user@.example.com',
      'user@_example.com',
      'user@exa_mple.com',
      'user@.com',
      'user@.co.uk',
      undefined,
    ]

    invalidEmails.forEach((email) => {
      expect(isValidEmail(email)).toBe(false)
    })
  })
})
