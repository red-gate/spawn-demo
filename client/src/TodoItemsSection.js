import React, { Component } from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'
import { withRouter } from 'react-router-dom'

import Fab from '@material-ui/core/Fab'
import RefreshIcon from '@material-ui/icons/Refresh'

import { fetchUser } from './modules/user'
import { fetchUserTodoItems, deleteTodoItem, createTodoItem, editTodoItem } from './modules/todo'

import { TodoItemsContainer } from './TodoItemsContainer'

import './TodoItemsSection.css'

class TodoItemsSection extends Component {

  constructor(props) {
    super(props)

    this.onFetchUserTodoItems = this.onFetchUserTodoItems.bind(this)
    this.onDeleteTodoItem = this.onDeleteTodoItem.bind(this)
    this.onCreateTodoItem = this.onCreateTodoItem.bind(this)
    this.onCheckedTodoItem = this.onCheckedTodoItem.bind(this)
  }

  onFetchUserTodoItems(username) {
    this.props.fetchUserTodoItems(username)
  }

  onDeleteTodoItem(item, username) {
    this.props.deleteTodoItem(item, username)
  }

  onCreateTodoItem(newTask, username) {
    this.props.createTodoItem({
      task: newTask
    }, username)
  }

  onCheckedTodoItem(item, username) {
    this.props.editTodoItem({
      ...item,
      done: !item.done,
    }, username)
  }

  render() {
    const { userTodoItems, username } = this.props

    return <div className='TodoItems-section'>
      <div className='TodoItems-header'>
        <h2>Todo items</h2>
        <Fab aria-label='Refresh' onClick={() => this.onFetchUserTodoItems(username)}>
          <RefreshIcon />
        </Fab>
      </div>
      <TodoItemsContainer
        todoItems={userTodoItems}
        onFetchTodoItems={() => this.onFetchUserTodoItems(username)}
        onCreateTodoItem={(item) => this.onCreateTodoItem(item, username)}
        onDeleteTodoItem={(item) => this.onDeleteTodoItem(item, username)}
        onCheckedTodoItem={(item) => this.onCheckedTodoItem(item, username)}
      />
    </div>
  }
}

const mapStateToProps = state => ({
  username: state.user.username,
  userTodoItems: state.todo.userTodoItems,
})

const mapDispatchToProps = dispatch => bindActionCreators({
  fetchUser,
  fetchUserTodoItems,
  deleteTodoItem,
  createTodoItem,
  editTodoItem,
}, dispatch)

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(TodoItemsSection))