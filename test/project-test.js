const { resetDataContainer } = require('./spawnctl')
const { createProject, fetchProjects, removeProject, updateProject } = require('./project-api')

const STARTING_NUMBER_OF_USER_PROJECTS = 2
const defaultAccount = 'spawn@red-gate.com'

var assert = require('assert')
describe('demo-app-projects', function () {
  this.timeout(0)

  const projectName = "sampleProject"

  beforeEach(function resetTodos(done) {
    resetDataContainer(process.env.todoContainerName, done)
  })

  beforeEach(function resetAccounts(done) {
    resetDataContainer(process.env.accountContainerName, done)
  })

  it('should create a project', async function () {
    const recordResp = await createProject(projectName, null, defaultAccount)

    assert.equal(recordResp.ok, true)

    const fetchResp = await fetchProjects(defaultAccount)

    assert.equal(fetchResp.length, STARTING_NUMBER_OF_USER_PROJECTS + 1)
  })

  it('should remove a project', async function () {
    const recordResp = await createProject(projectName, null, defaultAccount)
    const { id } = await recordResp.json()

    const deleteResp = await removeProject(id, defaultAccount)
    assert.equal(deleteResp.ok, true)

    const fetchResp = await fetchProjects(defaultAccount)
    assert.equal(fetchResp.length, STARTING_NUMBER_OF_USER_PROJECTS)
  })

  it('should update a project', async function () {
    const newProject = "newProject"
    
    const recordResp = await createProject(projectName, null, defaultAccount)
    const { id } = await recordResp.json()

    const updateResp = await updateProject(id, newProject, defaultAccount)
    assert.equal(updateResp.ok, true)

    const { name } = await updateResp.json()
    assert.equal(name, newProject)
  })

})