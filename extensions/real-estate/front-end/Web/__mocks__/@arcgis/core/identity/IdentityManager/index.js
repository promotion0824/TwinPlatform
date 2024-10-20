/**
 * https://jestjs.io/docs/manual-mocks#mocking-node-modules
 */
const IdentityManager = { registerToken: jest.fn() }
export default IdentityManager
