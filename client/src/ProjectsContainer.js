import React, { Component } from 'react'
import moment from 'moment'
import { Link } from 'react-router-dom'

import Fab from '@material-ui/core/Fab'
import Input from '@material-ui/core/Input'
import AddIcon from '@material-ui/icons/Add'
import DeleteIcon from '@material-ui/icons/Delete'
import ProjectIcon from '@material-ui/icons/Assignment'

import './ProjectsContainer.css'

export default class ProjectsContainer extends Component {

  constructor(props) {
    super(props)
    this.onProjectChange = this.onProjectChange.bind(this)
    this.onCreateProject = this.onCreateProject.bind(this)
    this.state = {
      newProject: null,
    }
  }

  onProjectChange(event) {
    this.setState({ newProject: event.target.value })
  }

  onCreateProject(event) {
    event.preventDefault()
    this.props.onCreateProject({
      name: this.state.newProject
    })
    this.setState({ newProject: null })
  }

  render() {
    const { projects, prefix } = this.props
    const sortedProjects = Object
      .keys(projects)
      .map((k, i) => projects[k])
      .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))

    const newProject = this.state.newProject || ''
    return <div className='Projects-container'>
      <div className='Projects-create'>
        <form onSubmit={this.onCreateProject}>
          <Input
            name='project'
            value={newProject}
            onChange={this.onProjectChange}
            placeholder='My project'
            inputProps={{
              'aria-label': 'Create new project',
            }}
          ></Input>
          <Fab color='primary' aria-label='Add' type='submit'>
            <AddIcon />
          </Fab>
        </form>
      </div>
      <div className='Projects-list'>
        {sortedProjects.map((v, i) => {
          return <div className='Projects-item' key={`project-${v.id}`}>
            <div className='Projects-item-name'>
              <ProjectIcon className='Projects-item-icon' />
              <Link to={prefix + `/projects/${v.id}`}>{v.name}</Link>
            </div>
            <div className='Projects-item-createdAt'>{moment(v.createdAt).calendar()}</div>
            <Fab aria-label='Delete' className='Projects-item-delete'>
              <DeleteIcon
                onClick={() => this.props.onDeleteProject(v)}>x</DeleteIcon>
            </Fab>
          </div>
        })}
      </div>
    </div>
  }
}
