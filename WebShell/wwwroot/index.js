// HTML Elements
const form = document.querySelector('#console')
const input = form.querySelector('#input')
const list = document.querySelector('.text')
const caret = document.querySelector('#caret')
const status = document.querySelector('#status')

// History
const history = loadHistory() || []
let flagHistory = false
let index = history.length ? history.length - 1 : 0

// WebSocket
let ws = null
const url = 'ws://localhost:5176/ws'
let stateWS = ''
let commandResult = ''

// Events
form.addEventListener('submit', handleForm)
input.addEventListener('keydown', handleKey)
input.addEventListener('input', handleInputStyle)
document.addEventListener('click', handleFocus)

function handleFocus() {
  input.focus()
}

function loadHistory() {
  return JSON.parse(sessionStorage.getItem('command'))
}

function saveHistory(history) {
  sessionStorage.setItem('command', JSON.stringify(history))
}

function handleInputStyle() {
  let len = input.value.length

  caret.style.left = `calc(16ch + ${len}ch)`
}

function keyArrowUp() {
  if (index === 0) return

  if (!flagHistory) {
    flagHistory = true
  } else {
    index--
  }
}

function keyArrowDown() {
  if (index === history.length - 1) return

  index++

  if (!history[index]) {
    flagHistory = false
    index = history.length - 1
  }
}

function handleKey(e) {
  if (!history.length) return

  if (e.code === 'ArrowUp' || e.code === 'ArrowDown') {
    if (e.code === 'ArrowUp') {
      e.preventDefault()
      keyArrowUp()
    } else if (flagHistory) {
      keyArrowDown()
    }

    input.value = history[index]
    handleInputStyle()
  }
}

function updateState() {
  if (ws) {
    switch (ws.readyState) {
      case WebSocket.CLOSED:
        stateWS = 'Disconnected'
        break
      case WebSocket.CLOSING:
        stateWS = 'Closing...'
        break
      case WebSocket.CONNECTING:
        stateWS = 'Connecting...'
        break
      case WebSocket.OPEN:
        stateWS = 'Connected'
        break
      default:
        stateWS = `Unknown WebSocket State: ${ws.readyState}`
        break
    }
    status.innerText = stateWS
  }
}

function connectWS() {
  if (!ws || ws.readyState !== WebSocket.OPEN) {
    ws = new WebSocket(url)
    ws.onopen = () => {
      updateState()
      console.log(stateWS)
    }
    ws.onclose = () => {
      updateState()
      console.log(stateWS)
    }
    ws.onerror = (e) => {
      updateState()
      console.log(e)
      console.log(`WebSocket error: ${e}`)
      commandResult = `WebSocket error: `
    }
    commandResult = 'Connect'

    ws.onmessage = (event) => {
      // commandResult = event.data
      list.lastChild.lastChild.innerText = event.data
    }
  } else {
    commandResult = 'Server is already started...'
  }
}

function disconnectWS() {
  if (!ws || ws.readyState !== WebSocket.OPEN) {
    commandResult = 'Server CMD not started'
  } else {
    ws.close()
    commandResult = 'Disconnect'
    console.log('Socket закрыт')
  }
}

function createList(command) {
  const li = document.createElement('li')
  const pre = document.createElement('pre')

  li.innerText = `$ user-command->${command}`
  pre.innerText = commandResult
  li.append(pre)
  list.append(li)
}

function handleForm(e) {
  e.preventDefault()
  const command = input.value.trim()

  commandResult = ''

  if (command) {
    switch (command) {
      case 'start':
        connectWS()
        break
      case 'exit':
        disconnectWS()
        break
      case 'clear':
        return list.innerHTML = ''
      case 'helpConsole':
        commandResult = `
      'start' -  Connect to server CMD
      'exit'  -  Disconnect server CMD`
        break
      default:
        if (ws && ws.readyState === WebSocket.OPEN) {
          ws.send(command)
        } else {
          commandResult = `'${command}' is not recognized as an internal command, enter 'helpConsole' for help.`
        }
        break
    }
    if (history[history.length - 1] !== command) {
      history.push(command)
      saveHistory(history)
      index = history.length - 1
    }
  }
  createList(command)

  form.reset()
  updateState()
  handleInputStyle()
}
