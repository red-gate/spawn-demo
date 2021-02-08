export function get(uri, username) {
  return fetch(uri, {
    method: 'GET',
    headers: {
      'Accept': 'application/json',
      'Authorization': `Basic ${getCredentials(username)}`
    }
  })
}

export function post(uri, payload, username) {
  return fetch(uri, {
    method: 'POST',
    body: JSON.stringify(payload),
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Basic ${getCredentials(username)}`
    }
  })
}

export function put(uri, payload, username) {
  return fetch(uri, {
    method: 'PUT',
    body: JSON.stringify(payload),
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Basic ${getCredentials(username)}`
    }
  })
}


export function remove(uri, payload, username) {
  return fetch(uri, {
    method: 'DELETE',
    body: JSON.stringify(payload),
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Basic ${getCredentials(username)}`
    }
  })
}

function getCredentials(username){
  const basicCredentials = `${username}:password`
  return btoa(basicCredentials)
}