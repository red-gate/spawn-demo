const { resetDataContainer } = require('./spawnctl')
const { createTodo, fetchTodos, removeTodo, updateTodo } = require('./todo-api')

const STARTING_NUMBER_OF_TODOS = 7
const defaultAccount = 'spawn@red-gate.com'

var assert = require('assert')
describe('demo-app-todos', function () {
  this.timeout(0)

  const testTask = 'Write the report';
  const testTaskDone = false;

  beforeEach(function (done) {
    resetDataContainer(process.env.todoContainerName, done)
  })
  
  it('should create a todo', async function () {
    const recordResp = await createTodo(testTask, testTaskDone, defaultAccount)

    assert.equal(recordResp.ok, true)

    const fetchResp = await fetchTodos(defaultAccount)

    assert.equal(fetchResp.length, STARTING_NUMBER_OF_TODOS + 1)
  })

  it('should remove a todo', async function () {
    const recordResp = await createTodo(testTask, testTaskDone, defaultAccount)
    const { id } = await recordResp.json()

    const deleteResp = await removeTodo(id, defaultAccount)
    assert.equal(deleteResp.ok, true)

    const fetchResp = await fetchTodos(defaultAccount)
    assert.equal(fetchResp.length, STARTING_NUMBER_OF_TODOS)
  })

  it('should update a todo', async function () {
    const newTask = "newTask"
    
    const recordResp = await createTodo(testTask, testTaskDone, defaultAccount)
    const { id } = await recordResp.json()

    const updateResp = await updateTodo(id, newTask, testTaskDone, defaultAccount)
    assert.equal(updateResp.ok, true)

    const { task } = await updateResp.json()
    assert.equal(task, newTask)
  })

})