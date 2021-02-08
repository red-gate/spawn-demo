import React, { Component } from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'
import { withRouter, Redirect } from 'react-router-dom'

import Fab from '@material-ui/core/Fab'
import RefreshIcon from '@material-ui/icons/Refresh'

import { fetchProjectTodoItems, deleteTodoItem, createTodoItem, editTodoItem } from './modules/todo'
import { fetchUserProjects, fetchOrganizationProjects, deleteProject, createProject } from './modules/projects'

import { TodoItemsContainer } from './TodoItemsContainer'

import './ProjectPage.css'

class ProjectPage extends Component {

  constructor(props) {
    super(props)

    this.onDeleteTodoItem = this.onDeleteTodoItem.bind(this)
    this.onCheckedTodoItem = this.onCheckedTodoItem.bind(this)

    this.onFetchProjectTodoItems = this.onFetchProjectTodoItems.bind(this)
    this.onCreateProjectTodoItem = this.onCreateProjectTodoItem.bind(this)

    const { match, username } = this.props
    const orgId = match && match.params && match.params.orgId
    const projectId = match && match.params && match.params.projectId

    if (orgId) {
      this.props.fetchOrganizationProjects(orgId, username)
    } else {
      this.props.fetchUserProjects(username)
    }
    this.props.fetchProjectTodoItems(projectId, username)
  }

  onFetchProjectTodoItems(project, username) {
    this.props.fetchProjectTodoItems(project.id, username)
  }

  onDeleteTodoItem(item, username) {
    this.props.deleteTodoItem(item, username)
  }

  onCreateProjectTodoItem(task, project, username) {
    this.props.createTodoItem({
      projectId: project.id,
      task,
    }, username)
  }

  onCheckedTodoItem(item, username) {
    this.props.editTodoItem({
      ...item,
      done: !item.done,
    }, username)
  }

  render() {
    const { projectTodoItems, match, userProjects, orgProjects, fetchInProgressUser, fetchInProgressOrg, username } = this.props
    const projectId = match && match.params && match.params.projectId
    const orgId = match && match.params && match.params.orgId

    if (orgId && fetchInProgressOrg !== false) {
      return <div className='Org-page-loading'>loading...</div>
    } else if (!orgId && fetchInProgressUser !== false) {
      return <div className='Org-page-loading'>loading...</div>
    }

    if (!projectId) {
      return <Redirect to='/' />
    }

    if (orgId) {
      const project = orgProjects[orgId][projectId]
      if (!project) {
        return <Redirect to='/' />
      }
      return <div className='Project-page'>
        <div className='Project-page-header'>
          <h2>{project.name}</h2>
          <Fab aria-label='Refresh' onClick={() => this.onFetchProjectTodoItems(project, username)}>
            <RefreshIcon />
          </Fab>
        </div>
        <TodoItemsContainer
          todoItems={projectTodoItems[project.id] || {}}
          onFetchTodoItems={() => this.onFetchProjectTodoItems(project, username)}
          onCreateTodoItem={(newTask) => this.onCreateProjectTodoItem(newTask, project, username)}
          onDeleteTodoItem={(item) => this.onDeleteTodoItem(item, username)}
          onCheckedTodoItem={(item) => this.onCheckedTodoItem(item, username)}
        />
      </div>
    }

    const project = userProjects[projectId]
    if (!project) {
      return <Redirect to='/' />
    }

    return <div className='Project-page'>
      <div className='Project-page-header'>
        <h2>{project.name}</h2>
        <Fab aria-label='Refresh' onClick={() => this.onFetchProjectTodoItems(project, username)}>
          <RefreshIcon />
        </Fab>
      </div>
      <TodoItemsContainer
        todoItems={projectTodoItems[project.id] || {}}
        onFetchTodoItems={() => this.onFetchProjectTodoItems(project, username)}
        onCreateTodoItem={(newTask) => this.onCreateProjectTodoItem(newTask, project, username)}
        onDeleteTodoItem={(item) => this.onDeleteTodoItem(item, username)}
        onCheckedTodoItem={(item) => this.onCheckedTodoItem(item, username)}
      />
    </div>
  }
}

const mapStateToProps = state => ({
  username: state.user.username,
  projectTodoItems: state.todo.projectTodoItems,
  userProjects: state.projects.userProjects,
  orgProjects: state.projects.orgProjects,
  fetchInProgressUser: state.projects.fetchInProgressUser,
  fetchInProgressOrg: state.projects.fetchInProgressOrg,
})

const mapDispatchToProps = dispatch => bindActionCreators({
  fetchProjectTodoItems,
  deleteTodoItem,
  createTodoItem,
  editTodoItem,
  fetchUserProjects,
  fetchOrganizationProjects,
  deleteProject,
  createProject,
}, dispatch)

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(ProjectPage))
