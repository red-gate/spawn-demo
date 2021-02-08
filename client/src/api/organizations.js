import 'whatwg-fetch'
import { get, post, remove, put } from './core'
import config from '../config'

const organizationsURL = (path) => {
  return `${config.spawn_demo_api_endpoint}${path}`
}

export const fetchOrganizations = (username) => {
  return get(organizationsURL('/api/organizations'), username)
}

export const fetchOrganization = (orgId, username) => {
  return get(organizationsURL('/api/organizations/' + orgId, username))
}

export const createOrganization = (payload, username) => {
  return post(organizationsURL('/api/organizations'), payload, username)
}

export const deleteOrganization = (payload, username) => {
  return remove(organizationsURL('/api/organizations'), payload, username)
}

export const editOrganization = (payload, username) => {
  return put(organizationsURL('/api/organizations', payload, username))
}

export const fetchOrganizationMembers = (orgId, username) => {
  return get(organizationsURL(`/api/organizations/${orgId}/members`), username)
}

export const memberLeave = (orgId, payload, username) => {
  return remove(organizationsURL(`/api/organizations/${orgId}/member`), payload, username)
}

export const memberJoin = (orgId, payload, username) => {
  return post(organizationsURL(`/api/organizations/${orgId}/member`), payload, username)
}