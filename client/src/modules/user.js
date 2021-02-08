import 'whatwg-fetch'

import { fetchAccounts, createAccount as createAccountAPI } from '../api/accounts'

export const USER_REQUESTED = 'user/USER_REQUESTED'
export const USER_COMPLETED = 'user/USER_COMPLETED'
export const USER_NOTFOUND = 'user/USER_NOTFOUND'
export const USER_FAILED = 'user/USER_FAILED'

export const USER_CREATE_REQUESTED = 'user/USER_CREATE_REQUESTED'
export const USER_CREATE_COMPLETED = 'user/USER_CREATE_COMPLETED'
export const USER_CREATE_FAILED = 'user/USER_CREATE_FAILED'

export const USER_LOGOUT = 'user/USER_LOGOUT'

const initialState = () => { return {
    username: localStorage.getItem('username'),
    user: null,
    loggedIn: false,
    loginInProgress: false,
    userNotFound: false,
  }
}

export default (state = initialState(), action) => {
  switch (action.type) {
    case USER_REQUESTED:
    case USER_CREATE_REQUESTED:
      return {
        ...state,
        loggedIn: false,
        loginInProgress: true,
        username: action.payload
      }
    case USER_CREATE_COMPLETED:
    case USER_COMPLETED:
      return {
        ...state,
        user: { ...action.payload },
        loggedIn: true,
        loginInProgress: false,
        userNotFound: false,
      }
    case USER_NOTFOUND:
      return {
        ...state,
        loggedIn: false,
        loginInProgress: false,
        userNotFound: true,
      }
    case USER_FAILED:
      return {
        ...state,
        loggedIn: false,
        loginInProgress: false,

      }
    case USER_LOGOUT:
      return initialState()
    default:
      return state
  }
}

export const fetchUser = (username) => {
  return (dispatch, getState) => {
    dispatch({ type: USER_REQUESTED, payload: username })
    fetchAccounts(username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
      })
      .then(json => {
        if (json && json.length && json.length === 1) {
          dispatch({
            type: USER_COMPLETED,
            payload: json[0]
          })
        } else {
          dispatch({ type: USER_NOTFOUND })
        }
      })
      .catch(error => {
        dispatch({ type: USER_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const createAccount = (payload) => {
  const username = payload.email
  localStorage.setItem('username', username)
  return (dispatch, getState) => {
    dispatch({ type: USER_CREATE_REQUESTED, payload: username })
    createAccountAPI(payload, username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
      })
      .then(json => {
        dispatch({
          type: USER_CREATE_COMPLETED,
          payload: json
        })
      })
      .catch(error => {
        dispatch({ type: USER_CREATE_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const logout = () => {
  return (dispatch, getState) => {
    localStorage.removeItem('username')
    dispatch({ type: USER_LOGOUT })
  }
}
