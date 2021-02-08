import 'whatwg-fetch'
import { get, post, remove, put } from './core'
import config from '../config'

const projectsURL = (path) => {
  return `${config.spawn_demo_api_endpoint}${path}`
}

export const fetchProjects = (username) => {
  return get(projectsURL('/api/projects'), username)
}

export const fetchUserProjects = (username) => {
  return get(projectsURL('/api/projects/user'), username)
}

export const fetchOrganizationProjects = (orgId, username) => {
  return get(projectsURL(`/api/projects/organization/${orgId}`), username)
}
export const createProject = (payload, username) => {
  return post(projectsURL('/api/projects'), payload, username)
}

export const deleteProject = (payload, username) => {
  return remove(projectsURL('/api/projects'), payload, username)
}

export const editProject = (payload, username) => {
  return put(projectsURL('/api/projects'), payload, username)
}