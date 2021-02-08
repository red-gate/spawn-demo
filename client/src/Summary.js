import React, { Component } from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'
import { Link } from 'react-router-dom'
import Card from '@material-ui/core/Card'
import CardContent from '@material-ui/core/CardContent'
import CardHeader from '@material-ui/core/CardHeader'
import CardActionArea from '@material-ui/core/CardActionArea'
import TodoIcon from '@material-ui/icons/DoneAll'
import OrgIcon from '@material-ui/icons/AccountBalance'
import ProjectIcon from '@material-ui/icons/Assignment'

import './Summary.css'

class Home extends Component {
  render() {
    return <div className="home-section">
      <div className="home-header">
        <h2>Summary</h2>
      </div>
      <div className="home-content">
        <Card>
          <CardActionArea component={Link} to="/user/todo-items">
            <CardHeader title="User items"/>
            <CardContent className="stat">
              <div className="icon"><TodoIcon fontSize='inherit'/></div>
              <div className="text">{this.props.numUserItems}</div>
            </CardContent>
          </CardActionArea>
        </Card>
        <Card>
          <CardActionArea component={Link} to="/user/projects">
            <CardHeader title="User Projects"/>
            <CardContent className="stat">
              <div className="icon"><ProjectIcon fontSize='inherit'/></div>
              <div className="text">{this.props.numProjects}</div>
            </CardContent>
          </CardActionArea>
        </Card>
        <Card>
          <CardActionArea component={Link} to="/organizations">
            <CardHeader title="Organisations"/>
            <CardContent className="stat">
              <div className="icon"><OrgIcon fontSize='inherit'/></div>
              <div className="text">{this.props.numOrganizations}</div>
            </CardContent>
          </CardActionArea>
        </Card>
      </div>
    </div>
  }
}

const mapStateToProps = state => ({
  numUserItems: Object.keys(state.todo.userTodoItems).length,
  numProjects: Object.keys(state.projects.userProjects).length,
  numOrganizations: Object.keys(state.organizations.orgs).length
})

const mapDispatchToProps = dispatch => bindActionCreators({}, dispatch)

export default connect(mapStateToProps, mapDispatchToProps)(Home)