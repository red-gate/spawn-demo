import React, { Component } from 'react'
import { Link, withRouter } from 'react-router-dom'

import Divider from '@material-ui/core/Divider'
import List from '@material-ui/core/List'
import ListItem from '@material-ui/core/ListItem'
import ListItemText from '@material-ui/core/ListItemText'
import AccountBoxIcon from '@material-ui/icons/AccountBox'
import ListItemIcon from '@material-ui/core/ListItemIcon'
import TodoIcon from '@material-ui/icons/DoneAll'
// import OrgIcon from '@material-ui/icons/AccountBalance'
import ProjectIcon from '@material-ui/icons/Assignment'
import Collapse from '@material-ui/core/Collapse'
import ExpandLess from '@material-ui/icons/ExpandLess'
import ExpandMore from '@material-ui/icons/ExpandMore'

import './Sidebar.css'

class Sidebar extends Component {
  constructor(props){
    super(props)
    this.state = {
      organisationsExpanded: true,
      userExpanded: true
    }
    this.handleOrganisationDropdownClick = this.handleOrganisationDropdownClick.bind(this)
    this.handleUserDropdownClick = this.handleUserDropdownClick.bind(this)
  }

  handleOrganisationDropdownClick(event){
    event.stopPropagation()
    event.preventDefault()
    this.setState(state => ({ organisationsExpanded: !state.organisationsExpanded }));
  }

  handleUserDropdownClick(event){
    event.stopPropagation()
    event.preventDefault()
    this.setState(state => ({ userExpanded: !state.userExpanded }));
  }

  render() {
    // const { organizations } = this.props
    const largerListItemStyle = {
      width: "15em",
    }
    const nestedListItemStyle = {
      ...largerListItemStyle,
      paddingLeft: "2em",
    }

    return <div className='Orgs-list'>
      <List component="nav">
        <ListItem button component={Link} to='/summary' style={largerListItemStyle} selected={this.props.location.pathname === '/summary'}>
          <ListItemIcon>
            <AccountBoxIcon />
          </ListItemIcon>
          <ListItemText primary={this.props.user} />
          {this.state.userExpanded ? <ExpandLess onClick={this.handleUserDropdownClick}/> : <ExpandMore onClick={this.handleUserDropdownClick}/>}
        </ListItem>
        <Collapse in={this.state.userExpanded} timeout="auto" unmountOnExit>
          <List disablePadding>
            <ListItem button component={Link} to='/user/todo-items' style={nestedListItemStyle} selected={this.props.location.pathname === '/user/todo-items'}>
              <ListItemIcon>
                  <TodoIcon />
              </ListItemIcon>
              <ListItemText primary="User items" />
            </ListItem>
            <ListItem button component={Link} to='/user/projects' style={nestedListItemStyle} selected={this.props.location.pathname === '/user/projects'}>
              <ListItemIcon>
                  <ProjectIcon />
              </ListItemIcon>
              <ListItemText primary="User Projects" />
            </ListItem>
          </List>
        </Collapse>
        <Divider style={{
          marginTop: "1em",
          marginBottom: "1em"
        }} />
        {/* <ListItem button component={Link} to='/organizations' selected={this.props.location.pathname === '/organizations'}>
          <ListItemIcon>
            <OrgIcon />
          </ListItemIcon>
          <ListItemText primary="Organizations" />
          {this.state.organisationsExpanded ? <ExpandLess onClick={this.handleOrganisationDropdownClick} /> : <ExpandMore onClick={this.handleOrganisationDropdownClick} />}
        </ListItem>
        <Collapse in={this.state.organisationsExpanded} timeout="auto" unmountOnExit>
          <List disablePadding>
            {Object.keys(organizations).map((k, i) => {
              const v = organizations[k]
              return (<ListItem button component={Link} to={`/organizations/${v.id}`} style={nestedListItemStyle} key={`todo-${v.id}`} selected={this.props.location.pathname.startsWith(`/organizations/${v.id}`)}>
                  <ListItemIcon>
                    <OrgIcon />
                  </ListItemIcon>
                  <ListItemText primary={v.name} />
                </ListItem>)
            })}
          </List>
        </Collapse> */}
      </List>
    </div>
  }
}

const SidebarComponent = withRouter(props => <Sidebar {...props}/>)

export default SidebarComponent