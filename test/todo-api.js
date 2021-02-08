const { sample_host } = require('./hostProvider')
const fetch = require("node-fetch")

async function createTodo(task, done, username) {
    const recordResp = await fetch(`http://${sample_host}/api/todo`, {
        method: 'POST',
        body: JSON.stringify({
            task: task,
            done: done
        }),
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(username + ':password').toString('base64')}`
        },
    })
    return recordResp
}

async function fetchTodos(username) {
    const fetchResp = await fetch(`http://${sample_host}/api/todo`, {
        method: 'GET',
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(username + ':password').toString('base64')}`
        }
    })
    .then(fetchResp => fetchResp.json())
    .then(data => {
        return data
    })
    return fetchResp
}

async function removeTodo(id, username) {
    const removeResp = await fetch(`http://${sample_host}/api/todo`,{
        method: 'DELETE',
        body: JSON.stringify({
            id: id
        }),
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(username + ':password').toString('base64')}`
        }
    })

    return removeResp
}

async function updateTodo(id, task, taskDone, username) {
    const updateResp = await fetch(`http://${sample_host}/api/todo`,{
        method: 'PUT',
        body: JSON.stringify({
            id: id,
            task: task,
            taskDone: taskDone
        }),
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(username + ':password').toString('base64')}`
        }
    })

    return updateResp
}

module.exports = {
    createTodo,
    fetchTodos,
    removeTodo,
    updateTodo
}