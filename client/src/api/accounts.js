import 'whatwg-fetch'
import { get, post, remove, put } from './core'
import config from '../config'

const accountURL = (path) => {
  return `${config.spawn_demo_api_endpoint}${path}`
}

export const fetchAccounts = (username) => {
  return get(accountURL('/api/accounts'), username)
}

export const createAccount = (payload, username) => {
  return post(accountURL('/api/accounts'), payload, username)
}

export const deleteAccount = (payload, username) => {
  return remove(accountURL('/api/accounts'), payload, username)
}

export const editAccount = (payload, username) => {
  return put(accountURL('/api/accounts'), payload, username)
}