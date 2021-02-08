const { resetDataContainer } = require('./spawnctl')
const { createAccount, fetchAccounts, removeAccount, updateAccount } = require('./account-api')

const STARTING_NUMBER_OF_USERS = 2

var assert = require('assert')
describe('demo-app-accounts', function () {
  this.timeout(0)

  const testEmail = "me@you.com"

  beforeEach(function (done) {
    resetDataContainer(process.env.accountContainerName, done)
  })

  it('should create an account', async function () {
    const recordResp = await createAccount(testEmail)

    assert.equal(recordResp.ok, true)

    const fetchResp = await fetchAccounts('spawn@red-gate.com')

    assert.equal(fetchResp.length, 1)
  })

  it('should remove an account', async function () {
    const recordResp = await createAccount(testEmail)
    assert.equal(recordResp.ok, true)

    const { id } = await recordResp.json()

    const deleteResp = await removeAccount(id, testEmail)
    assert.equal(deleteResp.ok, true)
  })

  it('should update an account', async function () {
    const newEmail = "new@email.com";
    
    const recordResp = await createAccount(newEmail)
    assert.equal(recordResp.ok, true)

    const { id } = await recordResp.json()

    const updateResp = await updateAccount(id, newEmail)
    assert.equal(updateResp.ok, true)

    const { email } = await updateResp.json();
    assert.equal(email, newEmail);
  })

})