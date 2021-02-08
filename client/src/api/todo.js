import 'whatwg-fetch'
import { get, post, remove, put } from './core'
import config from '../config'

const todoURL = (path) => {
  return `${config.spawn_demo_api_endpoint}${path}`
}

export const fetchTodos = (username) => {
  return get(todoURL('/api/todo'), username)
}

export const fetchUserTodos = (username) => {
  return get(todoURL('/api/todo/user'), username)
}

export const fetchProjectTodos = (projectId, username) => {
  return get(todoURL(`/api/todo/project/${projectId}`), username)
}

export const createTodo = (payload, username) => {
  return post(todoURL('/api/todo'), payload, username)
}

export const deleteTodo = (payload, username) => {
  return remove(todoURL('/api/todo'), payload, username)
}

export const editTodo = (payload, username) => {
  return put(todoURL('/api/todo'), payload, username)
}