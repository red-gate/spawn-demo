import { combineReducers } from 'redux'
import { connectRouter } from 'connected-react-router'

import user from './user'
import todo from './todo'
import organizations from './organizations'
import projects from './projects'

export default (history) => combineReducers({
  router: connectRouter(history),
  user,
  organizations,
  projects,
  todo,
})