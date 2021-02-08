import React, { Component } from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'

import { withRouter, Redirect } from 'react-router-dom'

import Fab from '@material-ui/core/Fab'
import RefreshIcon from '@material-ui/icons/Refresh'

import { createOrganization } from './modules/organizations'
import { fetchOrganizationProjects, deleteProject, createProject } from './modules/projects'

import ProjectsContainer from './ProjectsContainer'

import './OrganizationPage.css'

class OrganizationPage extends Component {

  constructor(props) {
    super(props)

    this.onFetchProjectTodoItems = this.onFetchProjectTodoItems.bind(this)
    this.onCreateProjectTodoItem = this.onCreateProjectTodoItem.bind(this)

    this.onCreateOrganizationProject = this.onCreateOrganizationProject.bind(this)

    this.onDeleteProject = this.onDeleteProject.bind(this)
    const { match, username } = this.props
    const orgId = match && match.params && match.params.orgId
    this.props.fetchOrganizationProjects(orgId, username)
  }

  componentDidUpdate(prevProps) {
    const { fetchInProgress, match, username } = this.props
    const prevMatch = prevProps.match
    const previousOrgId = prevMatch && prevMatch.params && prevMatch.params.orgId
    const newOrgId = match && match.params && match.params.orgId

    if(previousOrgId !== newOrgId){
      this.props.fetchOrganizationProjects(newOrgId, username)
    }
    else if (newOrgId && prevProps.fetchInProgress === true && fetchInProgress === false) {
      this.props.fetchOrganizationProjects(newOrgId, username)
    }
  }

  onFetchProjectTodoItems(project, username) {
    this.props.fetchProjectTodoItems(project.id, username)
  }

  onDeleteProject(project, username) {
    this.props.deleteProject(project, username)
  }

  onCreateProjectTodoItem(task, project, username) {
    this.props.createTodoItem({
      projectId: project.id,
      task,
    }, username)
  }

  onCreateOrganizationProject(newProject, org, username) {
    this.props.createProject({
      ...newProject,
      orgId: org.id,
    }, username)
  }

  render() {
    const { match, organizations, fetchInProgress, orgProjects, username } = this.props

    if (fetchInProgress === null || fetchInProgress === true) {
      return <div className='Org-page-loading'>loading...</div>
    }

    const orgId = match && match.params && match.params.orgId

    if (!orgId) {
      return <Redirect to='/' />
    }

    const org = organizations[orgId]
    if (!org) {
      return <Redirect to='/' />
    }

    return <div className='Org-page'>
      <div className='Org-page-header'>
        <h2>{org.name}</h2>
        <Fab aria-label='Refresh' onClick={() => this.props.fetchOrganizationProjects(org.id, username)}>
          <RefreshIcon />
        </Fab>
      </div>
      <ProjectsContainer
        projects={orgProjects[org.id] || {}}
        prefix={`/organizations/${org.id}`}
        onFetchProjects={() => this.props.fetchOrganizationProjects(org.id, username)}
        onDeleteProject={(project) => this.onDeleteProject(project, username)}
        onCreateProject={(newProject) => this.onCreateOrganizationProject(newProject, org, username)}
      />
    </div>
  }
}

const mapStateToProps = state => ({
  username: state.user.username,
  organizations: state.organizations.orgs,
  fetchInProgress: state.organizations.fetchInProgress,

  orgProjects: state.projects.orgProjects,
})

const mapDispatchToProps = dispatch => bindActionCreators({
  fetchOrganizationProjects,
  deleteProject,
  createOrganization,
  createProject,
}, dispatch)

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(OrganizationPage))
