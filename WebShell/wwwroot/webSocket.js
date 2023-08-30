const URL = 'ws://localhost:5176/ws'

 let ws = null
let stateWS = ''
 let commandResult = ''

 function updateState(status) {
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

 function connectWS(status) {
    if (!ws || ws.readyState !== WebSocket.OPEN) {
        ws = new WebSocket(URL)
        ws.onopen = () => {
            updateState(status)
            console.log(stateWS)
        }
        ws.onclose = () => {
            updateState(status)
            console.log(stateWS)
        }
        ws.onerror = (e) => {
            updateState(status)
            console.log(e)
            console.log(`WebSocket error: ${e}`)
            commandResult = `WebSocket error: `
        }
        commandResult = 'Connect'

        ws.onmessage = (event) => {
            list.lastChild.lastChild.innerText += event.data
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