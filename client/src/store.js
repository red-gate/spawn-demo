import { createStore, applyMiddleware, compose } from 'redux'
import logger from 'redux-logger'
import thunk from 'redux-thunk'
import { routerMiddleware } from 'connected-react-router'

import createHistory from 'history/createBrowserHistory'

import createRootReducer from './modules'

export const history = createHistory()

const initialState = {}

const store = createStore(
  createRootReducer(history),
  initialState,
  compose(
    applyMiddleware(
      routerMiddleware(history),
      thunk,
      logger
    )
  )
)

export default store