import React, { Component } from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'
import { withRouter, Link } from 'react-router-dom'
import moment from 'moment'

import Fab from '@material-ui/core/Fab'
import Input from '@material-ui/core/Input'
import AddIcon from '@material-ui/icons/Add'
import DeleteIcon from '@material-ui/icons/Delete'
import RefreshIcon from '@material-ui/icons/Refresh'
import OrgIcon from '@material-ui/icons/AccountBalance'

import { fetchOrganizations, deleteOrganization, createOrganization } from './modules/organizations'

import './OrganizationsSection.css'

class OrganizationsSection extends Component {
  constructor(props) {
    super(props)

    this.onFetchOrganizations = this.onFetchOrganizations.bind(this)
    this.onCreateOrganization = this.onCreateOrganization.bind(this)
    this.onDeleteOrganization = this.onDeleteOrganization.bind(this)
    this.onOrganizationChange = this.onOrganizationChange.bind(this)

    this.state = {
      newOrg: null,
    }
    this.props.fetchOrganizations(this.props.username)
  }

  onFetchOrganizations(username) {
    this.props.fetchOrganizations(username)
  }

  onDeleteOrganization(org, username) {
    this.props.deleteOrganization(org, username)
  }

  onCreateOrganization(event, username) {
    event.preventDefault()
    this.props.createOrganization({
      name: this.state.newOrg
    }, username)
    this.setState({ newOrg: null })
  }

  onOrganizationChange(event) {
    this.setState({ newOrg: event.target.value })
  }

  onCreateOrganizationProject(newProject, org, username) {
    this.props.createProject({
      ...newProject,
      orgId: org.id,
    }, username)
  }

  render() {
    const { organizations, username } = this.props
    const { newOrg } = this.state

    const sortedOrgs = Object
      .keys(organizations)
      .map((k, i) => organizations[k])
      .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))

    return <div className='Organizations-section'>
      <div className='Orgs-header'>
        <h2>Organizations</h2>
        <Fab aria-label='Refresh' onClick={() => this.onFetchOrganizations(username)}>
          <RefreshIcon />
        </Fab>
      </div>
      <div className='Orgs-create'>
        <form onSubmit={(event) => this.onCreateOrganization(event, username)}>
          <Input
            name='name'
            value={newOrg || ''}
            onChange={this.onOrganizationChange}
            placeholder='Redgate'
            inputProps={{
              'aria-label': 'Create new organization',
            }}
          ></Input>
          <Fab color='primary' aria-label='Add' type='submit'>
            <AddIcon />
          </Fab>
        </form>
      </div>
      <div className='Orgs-list'>
        {sortedOrgs.map((v, i) => {
          return <div className='Orgs-item' key={`todo-${v.id}`}>
            <div className='Orgs-item-name'>
              <OrgIcon className='Orgs-item-icon' />
              <Link to={`/organizations/${v.id}`}>{v.name}</Link>
            </div>
            <div className='Orgs-item-createdAt'>{moment(v.createdAt).calendar()}</div>
            <Fab aria-label='Delete' className='Orgs-item-delete'>
              <DeleteIcon
                onClick={() => this.onDeleteOrganization(v, username)}>x</DeleteIcon>
            </Fab>
          </div>
        })}
      </div>
    </div>
  }
}

const mapStateToProps = state => ({
  username: state.user.username,
  organizations: state.organizations.orgs,
})

const mapDispatchToProps = dispatch => bindActionCreators({
  fetchOrganizations,
  deleteOrganization,
  createOrganization,
}, dispatch)

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(OrganizationsSection))
