import 'whatwg-fetch'

import {
  createProject as createProjectAPI,
  deleteProject as deleteProjectAPI,
  fetchOrganizationProjects as fetchOrganizationProjectsAPI,
  fetchUserProjects as fetchUserProjectsAPI,
} from '../api/projects'

import { USER_LOGOUT } from './user'

export const USER_PROJECTS_REQUESTED = 'projects/USER_PROJECTS_REQUESTED'
export const USER_PROJECTS_COMPLETED = 'projects/USER_PROJECTS_COMPLETED'
export const USER_PROJECTS_NOTFOUND = 'projects/USER_PROJECTS_NOTFOUND'
export const USER_PROJECTS_FAILED = 'projects/USER_PROJECTS_FAILED'

export const ORG_PROJECTS_REQUESTED = 'projects/ORG_PROJECTS_REQUESTED'
export const ORG_PROJECTS_COMPLETED = 'projects/ORG_PROJECTS_COMPLETED'
export const ORG_PROJECTS_NOTFOUND = 'projects/ORG_PROJECTS_NOTFOUND'
export const ORG_PROJECTS_FAILED = 'projects/ORG_PROJECTS_FAILED'

export const PROJECTS_CREATE_REQUESTED = 'projects/PROJECTS_CREATE_REQUESTED'
export const PROJECTS_CREATE_COMPLETED = 'projects/PROJECTS_CREATE_COMPLETED'
export const PROJECTS_CREATE_FAILED = 'projects/PROJECTS_CREATE_FAILED'

export const PROJECTS_DEL_REQUESTED = 'projects/PROJECTS_DEL_REQUESTED'
export const PROJECTS_DEL_COMPLETED = 'projects/PROJECTS_DEL_COMPLETED'
export const PROJECTS_DEL_FAILED = 'projects/PROJECTS_DEL_FAILED'

const initialState = {
  userProjects: {},
  orgProjects: {},
  fetchInProgressUser: null,
  fetchInProgressOrg: null,
  deleteInProgress: false,
  createInProgress: false,
}


export default (state = initialState, action) => {
  switch (action.type) {
    case USER_PROJECTS_REQUESTED:
      return {
        ...state,
        fetchInProgressUser: true,
        userProjects: {}
      }
    case ORG_PROJECTS_REQUESTED:
      return {
        ...state,
        fetchInProgressOrg: true,
        userProjects: {}
      }
    case USER_PROJECTS_COMPLETED:
      return {
        ...state,
        fetchInProgressUser: false,
        userProjects: action.payload.projects.reduce((map, item) => {
          map[item.id] = item
          return map
        }, {}),
      }
    case ORG_PROJECTS_COMPLETED:
      return {
        ...state,
        fetchInProgressOrg: false,
        orgProjects: action.payload.projects.reduce((map, item) => {
          if (item.orgId in map) {
            map[item.orgId][item.id] = item
          } else {
            map[item.orgId] = {}
            map[item.orgId][item.id] = item
          }
          return map
        }, {}),
      }
    case PROJECTS_DEL_REQUESTED:
      return {
        ...state,
        deleteInProgress: true,
      }
    case PROJECTS_DEL_COMPLETED:
      {
        const userProjects = { ...state.userProjects }
        const orgProjects = { ...state.orgProjects }
        const project = action.payload.project
        const newUserProjects = {...userProjects}
        const newOrgProjects = {...orgProjects}

        if (project.orgId == null) {
          if (project.id in newUserProjects) {
            delete newUserProjects[project.id]
          }
        } else {
          if (project.orgId in newOrgProjects) {
            if (project.id in newOrgProjects[project.orgId]) {
              delete newOrgProjects[project.orgId][project.id]
            }
          }
        }
        return {
          ...state,
          deleteInProgress: false,
          userProjects: newUserProjects,
          orgProjects: newOrgProjects,
        }
      }
    case PROJECTS_DEL_FAILED:
      return {
        ...state,
        deleteInProgress: false,
        error: action.payload.error,
      }
    case PROJECTS_CREATE_REQUESTED:
      return {
        ...state,
        createInProgress: true,
      }
    case PROJECTS_CREATE_COMPLETED:
      {
        const userProjects = { ...state.userProjects }
        const orgProjects = { ...state.orgProjects }
        const newUserProjects = {...userProjects}
        const newOrgProjects = {...orgProjects}

        const project = action.payload.project

        if (project.orgId == null) {
          newUserProjects[project.id] = action.payload.project
        } else {
          if (project.orgId in newOrgProjects) {
            newOrgProjects[project.orgId][project.id] = project
          } else {
            newOrgProjects[project.orgId] = {}
            newOrgProjects[project.orgId][project.id] = project
          }
        }

        return {
          ...state,
          createInProgress: false,
          userProjects: newUserProjects,
          orgProjects: newOrgProjects,
        }
      }
    case PROJECTS_CREATE_FAILED:
      return {
        ...state,
        createInProgress: false,
        error: action.payload.error,
      }
    case USER_PROJECTS_FAILED:
      return {
        ...state,
        fetchInProgressUser: false,
        userProjects: {},
        error: action.payload.error,
      }
    case ORG_PROJECTS_FAILED:
      return {
        ...state,
        fetchInProgressOrg: false,
        userProjects: {},
        error: action.payload.error,
      }
    case USER_LOGOUT:
        return initialState
    default:
      return state
  }
}

export const fetchUserProjects = (username) => {
  return (dispatch, getState) => {
    dispatch({ type: USER_PROJECTS_REQUESTED })
    fetchUserProjectsAPI(username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
      })
      .then(json => {
        dispatch({
          type: USER_PROJECTS_COMPLETED,
          payload: {
            projects: json
          }
        })
      })
      .catch(error => {
        dispatch({ type: USER_PROJECTS_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const fetchOrganizationProjects = (orgId, username) => {
  return (dispatch, getState) => {
    dispatch({ type: ORG_PROJECTS_REQUESTED })
    fetchOrganizationProjectsAPI(orgId, username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
      })
      .then(json => {
        dispatch({
          type: ORG_PROJECTS_COMPLETED,
          payload: {
            projects: json
          }
        })
      })
      .catch(error => {
        dispatch({ type: ORG_PROJECTS_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const createProject = (project, username) => {
  return (dispatch, getState) => {
    dispatch({ type: PROJECTS_CREATE_REQUESTED })
    createProjectAPI(project, username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
        dispatch({ type: PROJECTS_CREATE_FAILED, payload: { error: 'unknown error' } })
      })
      .then(json => {
        dispatch({ type: PROJECTS_CREATE_COMPLETED, payload: { project: json } })
      })
      .catch(error => {
        dispatch({ type: PROJECTS_CREATE_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const deleteProject = (project) => {
  return (dispatch, getState) => {
    dispatch({ type: PROJECTS_DEL_REQUESTED })
    deleteProjectAPI(project)
      .then(resp => {
        if (resp.status === 200) {
          dispatch({ type: PROJECTS_DEL_COMPLETED, payload: { project } })
          return
        }
        dispatch({ type: PROJECTS_DEL_FAILED, payload: { error: 'unknown error' } })
      })
      .catch(error => {
        dispatch({ type: PROJECTS_DEL_FAILED, payload: { error } })
        console.error(error)
      })
  }
}
