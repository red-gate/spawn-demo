import 'whatwg-fetch'

import {
  fetchOrganizations as fetchOrganizationsAPI,
  createOrganization as createOrganizationAPI,
  deleteOrganization as deleteOrganizationAPI,
} from '../api/organizations'

import { USER_LOGOUT } from './user'

export const ORGS_REQUESTED = 'orgs/ORGS_REQUESTED'
export const ORGS_COMPLETED = 'orgs/ORGS_COMPLETED'
export const ORGS_NOTFOUND = 'orgs/ORGS_NOTFOUND'
export const ORGS_FAILED = 'orgs/ORGS_FAILED'

export const ORGS_CREATE_REQUESTED = 'orgs/ORGS_CREATE_REQUESTED'
export const ORGS_CREATE_COMPLETED = 'orgs/ORGS_CREATE_COMPLETED'
export const ORGS_CREATE_FAILED = 'orgs/ORGS_CREATE_FAILED'

export const ORGS_DEL_REQUESTED = 'orgs/ORGS_DEL_REQUESTED'
export const ORGS_DEL_COMPLETED = 'orgs/ORGS_DEL_COMPLETED'
export const ORGS_DEL_FAILED = 'orgs/ORGS_DEL_FAILED'

const initialState = {
  orgs: {},
  fetchInProgress: null,
  deleteInProgress: null,
  createInProgress: null,
}


export default (state = initialState, action) => {
  switch (action.type) {
    case ORGS_REQUESTED:
      return {
        ...state,
        fetchInProgress: true,
        organizations: {}
      }
    case ORGS_COMPLETED:
      return {
        ...state,
        fetchInProgress: false,
        orgs: action.payload.orgs.reduce((map, item) => {
          map[item.id] = item
          return map
        }, {}),
      }
    case ORGS_DEL_REQUESTED:
      return {
        ...state,
        deleteInProgress: true,
      }
    case ORGS_DEL_COMPLETED:
      {
        const orgs = { ...state.orgs }
        const newOrgs = {...orgs}
        delete newOrgs[action.payload.org.id]
        return {
          ...state,
          deleteInProgress: false,
          orgs: newOrgs,
        }
      }
    case ORGS_DEL_FAILED:
      return {
        ...state,
        deleteInProgress: false,
        error: action.payload.error,
      }
    case ORGS_CREATE_REQUESTED:
      return {
        ...state,
        createInProgress: true,
      }
    case ORGS_CREATE_COMPLETED:
      {
        const orgs = { ...state.orgs }
        const newOrgs = {...orgs}
        newOrgs[action.payload.org.id] = action.payload.org

        return {
          ...state,
          createInProgress: false,
          orgs: newOrgs,
        }
      }
    case ORGS_CREATE_FAILED:
      return {
        ...state,
        createInProgress: false,
        error: action.payload.error,
      }
    case ORGS_FAILED:
      return {
        ...state,
        fetchInProgress: false,
        orgs: {},
        error: action.payload.error,
      }
    case USER_LOGOUT:
        return initialState
    default:
      return state
  }
}

export const fetchOrganizations = (username) => {
  return (dispatch, getState) => {
    dispatch({ type: ORGS_REQUESTED })
    fetchOrganizationsAPI(username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
      })
      .then(json => {
        dispatch({
          type: ORGS_COMPLETED,
          payload: {
            orgs: json
          }
        })
      })
      .catch(error => {
        dispatch({ type: ORGS_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const createOrganization = (org, username) => {
  return (dispatch, getState) => {
    dispatch({ type: ORGS_CREATE_REQUESTED })
    createOrganizationAPI(org, username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
        dispatch({ type: ORGS_CREATE_FAILED, payload: { error: 'unknown error' } })
      })
      .then(json => {
        dispatch({ type: ORGS_CREATE_COMPLETED, payload: { org: json } })
      })
      .catch(error => {
        dispatch({ type: ORGS_CREATE_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const deleteOrganization = (org, username) => {
  return (dispatch, getState) => {
    dispatch({ type: ORGS_DEL_REQUESTED })
    deleteOrganizationAPI(org, username)
      .then(resp => {
        if (resp.status === 200) {
          dispatch({ type: ORGS_DEL_COMPLETED, payload: { org } })
          return
        }
        dispatch({ type: ORGS_DEL_FAILED, payload: { error: 'unknown error' } })
      })
      .catch(error => {
        dispatch({ type: ORGS_DEL_FAILED, payload: { error } })
        console.error(error)
      })
  }
}
