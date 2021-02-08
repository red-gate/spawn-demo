const { sample_host } = require('./hostProvider')
const fetch = require("node-fetch")

async function createProject(projectName, id, username) {
    const recordResp = await fetch(`http://${sample_host}/api/projects`, {
        method: 'POST',
        body: JSON.stringify({
            name: projectName,
            orgId: id
        }),
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(username + ':password').toString('base64')}`
        },
    })
    return recordResp
}

async function fetchProjects(username) {
    const fetchResp = await fetch(`http://${sample_host}/api/projects`, {
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

async function fetchProjectsByOrganizationId(id, username) {
    const fetchResp = await fetch(`http://${sample_host}/api/projects/organization/${id}`, {
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

async function removeProject(id, username) {
    const removeResp = await fetch(`http://${sample_host}/api/projects`,{
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

async function updateProject(id, name, username) {
    const updateResp = await fetch(`http://${sample_host}/api/projects`,{
        method: 'PUT',
        body: JSON.stringify({
            id: id,
            name: name
        }),
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(username + ':password').toString('base64')}`
        }
    })

    return updateResp
}

module.exports = {
    createProject,
    fetchProjects,
    fetchProjectsByOrganizationId,
    removeProject,
    updateProject
}