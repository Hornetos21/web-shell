const URL = 'ws://localhost:5176/ws'

let ws = null
let stateWS = 'Disconnected'
let commandResult = ''


const updateState = () => {
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
    }
    status.innerText = stateWS
}

function connectWS() {
    if (!ws || ws.readyState !== WebSocket.OPEN) {
        ws = new WebSocket(URL)
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
            switch (event.data) {
                case 'CAN_WRITE_FALSE':
                    canWrite = false
                    caret.classList.add('hide')
                    break
                case 'CAN_WRITE_TRUE':
                    canWrite = true
                    caret.classList.remove('hide')
                   
                    break
                default :
                    list.lastChild.lastChild.innerText += event.data
                    break
            }
            input.disabled = !canWrite
            input.focus()
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