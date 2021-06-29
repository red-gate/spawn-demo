import React, { Component } from 'react'
import { bindActionCreators } from 'redux'
import { connect } from 'react-redux'

import { withRouter } from 'react-router-dom'
import { Route, Switch, Redirect } from 'react-router'

import AppBar from '@material-ui/core/AppBar'
import SpaOutlined from '@material-ui/icons/SpaOutlined'
import CssBaseline from '@material-ui/core/CssBaseline'
import Toolbar from '@material-ui/core/Toolbar'
import Typography from '@material-ui/core/Typography'
import Button from '@material-ui/core/Button'

import { fetchUser, createAccount, logout } from './modules/user'
import { fetchUserTodoItems } from './modules/todo'
import { fetchOrganizations } from './modules/organizations'
import { fetchUserProjects } from './modules/projects'

import './App.css'

import Summary from './Summary'
import Login from './Login'
import OrganizationPage from './OrganizationPage'
import OrganizationsSection from './OrganizationsSection'
import ProjectPage from './ProjectPage'
import ProjectsSection from './ProjectsSection'
import Sidebar from './Sidebar'
import TodoItemsSection from './TodoItemsSection'

class App extends Component {
  constructor(props) {
    super(props)

    if(this.props.username){
      this.props.fetchUser(this.props.username)
    }

    this.gotoHome = this.gotoHome.bind(this)
    this.onLogout = this.onLogout.bind(this)
  }

  componentWillUpdate(nextProps){
    if(this.props.loginInProgress && nextProps.loggedIn){
      this.props.fetchUserTodoItems(nextProps.username)
      this.props.fetchOrganizations(nextProps.username)
      this.props.fetchUserProjects(nextProps.username)
    }
  }

  gotoHome() {
    this.props.history.push('/')
  }

  onLogout() {
    this.props.logout()
    this.gotoHome()
  }

  renderUserInfo(){
    if(this.props.loggedIn){
      return <div className='user-info'>
        <div className='App-bar-username'>{this.props.user && this.props.user.email}</div>
        <Button
          variant="outlined"
          color="inherit"
          onClick={this.onLogout}>
          Log out
        </Button>
      </div>
    }

    return null
  }

  render() {
    const { user, organizations, loggedIn } = this.props

    return (
      <div className="App">
        <CssBaseline />
        <AppBar position='static'>
          <Toolbar>
            <SpaOutlined />
            <Typography variant="h6" color="inherit" noWrap>
              <div className='App-bar-name' onClick={this.gotoHome}>Todo list</div>
            </Typography>
            { this.renderUserInfo() }
          </Toolbar>

        </AppBar>
        {!loggedIn ?
          <Login login={this.props.createAccount}/>
          : <div className='App-content'>
            <div className='App-sidebar'>
              <Sidebar organizations={organizations} user={user && user.email} />
            </div>
            <Switch>
              <Route exact path='/summary' component={Summary} />
              <Route exact path='/user/todo-items' component={TodoItemsSection} />
              <Route exact path='/user/projects' component={ProjectsSection} />
              <Route exact path='/user/projects/:projectId' component={ProjectPage} />
              <Route exact path='/organizations' component={OrganizationsSection} />
              <Route exact path='/organizations/:orgId/projects/:projectId' component={ProjectPage} />
              <Route exact path='/organizations/:orgId' component={OrganizationPage} />
              <Route render={() => <Redirect to="/user/todo-items"/>}/>
            </Switch>
          </div>}
        <a href="https://spawn.cc" target="_blank" className='App-footer'>Visit the Spawn homepage</a>

      </div >
    )
  }
}

const mapStateToProps = state => ({
  user: state.user.user,
  username: state.user.username,
  loggedIn: state.user.loggedIn,
  loginInProgress: state.user.loginInProgress,
  organizations: state.organizations.orgs
})

const mapDispatchToProps = dispatch => bindActionCreators({
  fetchUser,
  createAccount,
  fetchUserTodoItems,
  fetchOrganizations,
  fetchUserProjects,
  logout
}, dispatch)

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(App))
