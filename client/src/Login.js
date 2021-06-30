import React, { Component } from 'react'

import Button from '@material-ui/core/Button'
import CssBaseline from '@material-ui/core/CssBaseline'
import TextField from '@material-ui/core/TextField'

import './Login.css'

class Login extends Component {

  constructor(props) {
    super(props)
    this.onLogin = this.onLogin.bind(this)
  }

  onLogin(event) {
    event.preventDefault()
    this.props.login({
      email: event.target.username.value
    })
    event.target.username.value = ''
  }

  render(){
    return (
      <div className='Account-login'>
        <CssBaseline />
        <div className='Account-login-header'>
          <h2>Sign in</h2>
        </div>
        <form noValidate onSubmit={e => this.onLogin(e)}>
          <TextField
            variant="outlined"
            margin="normal"
            required
            fullWidth
            id="username"
            label="Email Address"
            name="username"
            defaultValue="spawn@red-gate.com"
            autoFocus
          />
          <Button
            className="login-button"
            type="submit"
            fullWidth
            variant="contained"
            color="primary"
          >
            Sign In
          </Button>
        </form>
      </div>
    )
  }
}

export default Login