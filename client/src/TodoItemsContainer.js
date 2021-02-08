import React, { Component } from 'react'
import moment from 'moment'

import Checkbox from '@material-ui/core/Checkbox'
import Fab from '@material-ui/core/Fab'
import Input from '@material-ui/core/Input'
import AddIcon from '@material-ui/icons/Add'
import DeleteIcon from '@material-ui/icons/Delete'

import './TodoItemsContainer.css'

export class TodoItemsContainer extends Component {
  constructor(props) {
    super(props)
    this.onTaskChange = this.onTaskChange.bind(this)
    this.onCreateTodoItem = this.onCreateTodoItem.bind(this)
    this.state = {
      newTask: null,
    }
  }
  onTaskChange(event) {
    this.setState({ newTask: event.target.value })
  }
  onCreateTodoItem(e) {
    e.preventDefault()
    const { newTask } = this.state
    if (newTask != null) {
      this.props.onCreateTodoItem(this.state.newTask)
      this.setState({ newTask: null })
    }
  }
  render() {
    const { todoItems } = this.props
    const sortedTodoItems = Object
      .keys(todoItems)
      .map((k, i) => todoItems[k])
      .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))

    const newTask = this.state.newTask || ''
    return <div className='TodoItems-container'>
      <div className='TodoItems-create'>
        <form onSubmit={this.onCreateTodoItem}>
          <Input
            name='task'
            value={newTask}
            onChange={this.onTaskChange}
            placeholder='My task'
            inputProps={{
              'aria-label': 'Create new task',
            }}
          ></Input>
          <Fab color='primary' aria-label='Add' type='submit'>
            <AddIcon />
          </Fab>
        </form>
      </div>
      <div className='TodoItems-list'>
        {sortedTodoItems.map((v, i) => {
          return <div className='TodoItems-item' key={`todo-${v.id}`}>
            <form className='TodoItems-item-checkbox'>
              <Checkbox
                onChange={() => this.props.onCheckedTodoItem(v)}
                checked={v.done}></Checkbox>
            </form>
            <div className='TodoItems-item-task'>{v.task}</div>
            <div className='TodoItems-item-createdAt'>{moment(v.createdAt).calendar()}</div>
            <Fab aria-label='Delete' className='TodoItems-item-delete'>
              <DeleteIcon
                onClick={() => this.props.onDeleteTodoItem(v)}>x</DeleteIcon>
            </Fab>
          </div>
        })}
      </div>
    </div>
  }
}
