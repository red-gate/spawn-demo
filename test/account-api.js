const { sample_host } = require('./hostProvider')
const fetch = require("node-fetch")

async function createAccount(email) {
    const recordResp = await fetch(`http://${sample_host}/api/accounts`, {
        method: 'POST',
        body: JSON.stringify({
            email: email
        }),
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(email + ':password').toString('base64')}`
        },
    })
    return recordResp
}

async function fetchAccounts(username) {
    const fetchResp = await fetch(`http://${sample_host}/api/accounts`, {
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

async function removeAccount(id, username) {
    const removeResp = await fetch(`http://${sample_host}/api/accounts`,{
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

async function updateAccount(id, email) {
    const updateResp = await fetch(`http://${sample_host}/api/accounts`,{
        method: 'PUT',
        body: JSON.stringify({
            id: id,
            email: email
        }),
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            "Authorization": `Basic ${Buffer.from(email + ':password').toString('base64')}`
        }
    })

    return updateResp
}

module.exports = {
    createAccount,
    fetchAccounts,
    removeAccount,
    updateAccount
}