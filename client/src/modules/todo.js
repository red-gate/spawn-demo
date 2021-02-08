import 'whatwg-fetch'

import {
  fetchProjectTodos,
  fetchUserTodos,
  createTodo,
  deleteTodo,
  editTodo,
} from '../api/todo'

import { USER_LOGOUT } from './user'

export const TODO_USER_REQUESTED = 'todo/TODO_USER_REQUESTED'
export const TODO_USER_COMPLETED = 'todo/TODO_USER_COMPLETED'
export const TODO_USER_FAILED = 'todo/TODO_USER_FAILED'

export const TODO_PROJECT_REQUESTED = 'todo/TODO_PROJECT_REQUESTED'
export const TODO_PROJECT_COMPLETED = 'todo/TODO_PROJECT_COMPLETED'
export const TODO_PROJECT_FAILED = 'todo/TODO_PROJECT_FAILED'

export const TODO_CREATE_REQUESTED = 'todo/TODO_CREATE_REQUESTED'
export const TODO_CREATE_COMPLETED = 'todo/TODO_CREATE_COMPLETED'
export const TODO_CREATE_FAILED = 'todo/TODO_CREATE_FAILED'

export const TODO_DEL_REQUESTED = 'todo/TODO_DEL_REQUESTED'
export const TODO_DEL_COMPLETED = 'todo/TODO_DEL_COMPLETED'
export const TODO_DEL_FAILED = 'todo/TODO_DEL_FAILED'

export const TODO_EDIT_REQUESTED = 'todo/TODO_EDIT_REQUESTED'
export const TODO_EDIT_COMPLETED = 'todo/TODO_EDIT_COMPLETED'
export const TODO_EDIT_FAILED = 'todo/TODO_EDIT_FAILED'


const initialState = {
  userTodoItems: {},
  projectTodoItems: {},
  fetchInProgress: false,
  deleteInProgress: false,
  createInProgress: false,
  editInProgress: false,
}

export default (state = initialState, action) => {
  switch (action.type) {
    case TODO_USER_REQUESTED:
      return {
        ...state,
        fetchInProgress: true,
        userTodoItems: {}
      }
    case TODO_PROJECT_REQUESTED:
      return {
        ...state,
        fetchInProgress: true,
        projectTodoItems: {}
      }
    case TODO_USER_COMPLETED:
      return {
        ...state,
        fetchInProgress: false,
        userTodoItems: action.payload.todoItems.reduce((map, item) => {
          map[item.id] = item
          return map
        }, {}),
      }
    case TODO_PROJECT_COMPLETED:
      {
        const newProjectTodoItems = action.payload.todoItems.reduce((map, item) => {

          if (item.projectId in map) {
            map[item.projectId][item.id] = item
          } else {
            map[item.projectId] = {}
            map[item.projectId][item.id] = item
          }

          return map
        }, {})

        return {
          ...state,
          fetchInProgress: false,
          projectTodoItems: {
            ...state.projectTodoItems,
            ...newProjectTodoItems,
          }
        }
      }
    case TODO_DEL_REQUESTED:
      return {
        ...state,
        deleteInProgress: true,
      }
    case TODO_DEL_COMPLETED:
      {
        const { userTodoItems, projectTodoItems } = { ...state }
        const { item } = action.payload
        const newUserTodoItems = {...userTodoItems}
        const newProjectTodoItems = {...projectTodoItems}

        if (item.id in newUserTodoItems) {
          delete newUserTodoItems[item.id]
        }
        if (item.projectId in newProjectTodoItems) {
          if (item.id in newProjectTodoItems[item.projectId]) {
            delete newProjectTodoItems[item.projectId][item.id]
          }
        }

        return {
          ...state,
          deleteInProgress: false,
          userTodoItems: newUserTodoItems,
          projectTodoItems: newProjectTodoItems,
        }
      }
    case TODO_DEL_FAILED:
      return {
        ...state,
        deleteInProgress: false,
        error: action.payload.error,
      }
    case TODO_EDIT_REQUESTED:
      return {
        ...state,
        editInProgress: true,
      }
    case TODO_EDIT_COMPLETED:
      {
        const { userTodoItems, projectTodoItems } = { ...state }
        const { item } = action.payload
        const newUserTodoItems = {...userTodoItems}
        const newProjectTodoItems = {...projectTodoItems}

        if (item.id in newUserTodoItems) {
          newUserTodoItems[item.id] = item
        }
        if (item.projectId in newProjectTodoItems) {
          if (item.id in newProjectTodoItems[item.projectId]) {
            newProjectTodoItems[item.projectId][item.id] = item
          }
        }

        return {
          ...state,
          editInProgress: false,
          userTodoItems: newUserTodoItems,
          projectTodoItems: newProjectTodoItems,
        }
      }
    case TODO_EDIT_FAILED:
      return {
        ...state,
        editInProgress: false,
      }
    case TODO_CREATE_REQUESTED:
      return {
        ...state,
        createInProgress: true,
      }
    case TODO_CREATE_COMPLETED:
      {
        const { userTodoItems, projectTodoItems } = { ...state }
        const { item } = action.payload
        const newUserTodoItems = {...userTodoItems}
        const newProjectTodoItems = {...projectTodoItems}

        if (!item.projectId) {
          newUserTodoItems[item.id] = item
        } else {
          if (item.projectId in newProjectTodoItems) {
            newProjectTodoItems[item.projectId][item.id] = item
          } else {
            newProjectTodoItems[item.projectId] = {}
            newProjectTodoItems[item.projectId][item.id] = item
          }
        }

        return {
          ...state,
          createInProgress: false,
          userTodoItems: newUserTodoItems,
          projectTodoItems: newProjectTodoItems,
        }
      }
    case TODO_CREATE_FAILED:
      return {
        ...state,
        createInProgress: false,
        error: action.payload.error,
      }
    case TODO_USER_FAILED:
      return {
        ...state,
        fetchInProgress: false,
        userTodoItems: {},
        error: action.payload.error,
      }
    case TODO_PROJECT_FAILED:
      return {
        ...state,
        fetchInProgress: false,
        projectTodoItems: {},
        error: action.payload.error,
      }
    case USER_LOGOUT:
      return initialState
    default:
      return state
  }
}

export const fetchUserTodoItems = (username) => {
  return (dispatch, getState) => {
    dispatch({ type: TODO_USER_REQUESTED })
    fetchUserTodos(username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
      })
      .then(json => {
        dispatch({
          type: TODO_USER_COMPLETED,
          payload: {
            todoItems: json
          }
        })
      })
      .catch(error => {
        dispatch({ type: TODO_USER_FAILED, payload: { error } })
        console.error(error)
      })
  }
}
export const fetchProjectTodoItems = (projectId, username) => {
  return (dispatch, getState) => {
    dispatch({ type: TODO_PROJECT_REQUESTED })
    fetchProjectTodos(projectId, username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
      })
      .then(json => {
        dispatch({
          type: TODO_PROJECT_COMPLETED,
          payload: {
            todoItems: json
          }
        })
      })
      .catch(error => {
        dispatch({ type: TODO_PROJECT_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const createTodoItem = (item, username) => {
  return (dispatch, getState) => {
    dispatch({ type: TODO_CREATE_REQUESTED })
    createTodo(item, username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
        dispatch({ type: TODO_CREATE_FAILED, payload: { error: 'unknown error' } })
      })
      .then(json => {
        dispatch({ type: TODO_CREATE_COMPLETED, payload: { item: json } })
      })
      .catch(error => {
        dispatch({ type: TODO_CREATE_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const deleteTodoItem = (item, username) => {
  return (dispatch, getState) => {
    dispatch({ type: TODO_DEL_REQUESTED })
    deleteTodo(item, username)
      .then(resp => {
        if (resp.status === 200) {
          dispatch({ type: TODO_DEL_COMPLETED, payload: { item } })
          return
        }
        dispatch({ type: TODO_DEL_FAILED, payload: { error: 'unknown error' } })
      })
      .catch(error => {
        dispatch({ type: TODO_DEL_FAILED, payload: { error } })
        console.error(error)
      })
  }
}

export const editTodoItem = (item, username) => {
  return (dispatch, getState) => {
    dispatch({ type: TODO_EDIT_REQUESTED })
    editTodo(item, username)
      .then(resp => {
        if (resp.status === 200) {
          return resp.json()
        }
        dispatch({ type: TODO_EDIT_FAILED, payload: { error: 'unknown error' } })
      })
      .then(json => {
        dispatch({ type: TODO_EDIT_COMPLETED, payload: { item: json } })
      })
      .catch(error => {
        dispatch({ type: TODO_EDIT_FAILED, payload: { error } })
        console.error(error)
      })
  }
}
