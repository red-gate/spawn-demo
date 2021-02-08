import React, { Component } from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'
import { withRouter } from 'react-router-dom'

import Fab from '@material-ui/core/Fab'
import RefreshIcon from '@material-ui/icons/Refresh'

import { fetchUserProjects, deleteProject, createProject } from './modules/projects'

import ProjectsContainer from './ProjectsContainer'

import './ProjectsSection.css'

class ProjectsSection extends Component {

  constructor(props) {
    super(props)

    this.onCreateUserProject = this.onCreateUserProject.bind(this)
    this.onDeleteProject = this.onDeleteProject.bind(this)
    this.props.fetchUserProjects(this.props.username)

    this.state = {
      newOrg: null,
    }
  }

  onDeleteProject(project, username) {
    this.props.deleteProject(project, username)
  }

  onCreateUserProject(newProject, username) {
    this.props.createProject(newProject, username)
  }

  render() {
    const { userProjects, username } = this.props
    return <div className='Projects-section'>
      <div className='Projects-header'>
        <h2>Projects</h2>
        <Fab aria-label='Refresh' onClick={() => this.props.fetchUserProjects(username)}>
          <RefreshIcon />
        </Fab>
      </div>
      <ProjectsContainer
        projects={userProjects || {}}
        prefix={`/user`}
        onFetchProjects={() => this.props.fetchUserProjects(username)}
        onDeleteProject={(project) => this.onDeleteProject(project, username)}
        onCreateProject={(newProject) => this.onCreateUserProject(newProject, username)}
        onDeleteTodoItem={(item) => this.onDeleteTodoItem(item, username)}
      />
    </div>
  }
}

const mapStateToProps = state => ({
  username: state.user.username,
  userProjects: state.projects.userProjects,
})

const mapDispatchToProps = dispatch => bindActionCreators({
  fetchUserProjects,
  deleteProject,
  createProject,
}, dispatch)

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(ProjectsSection))