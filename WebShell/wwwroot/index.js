// HTML Elements
const form = document.querySelector('#console')
const input = form.querySelector('#input')
const list = document.querySelector('.text')
const caret = document.querySelector('.caret')
const status = document.querySelector('#status')

// History
const history = loadHistory() || []
let flagHistory = false
let index = history.length ? history.length - 1 : 0

// Events
form.addEventListener('submit', handleForm)
input.addEventListener('keydown', handleKey)
input.addEventListener('input', handleInputStyle)
document.addEventListener('click', handleFocus)
document.addEventListener('mouseenter', () => caret.classList.add('caret'))
document.addEventListener('mouseleave', () => caret.classList.remove('caret'))

// TODO Send Ctrl+C for cancel process 
/*document.addEventListener('keydown', handleCancelCommand)

function handleCancelCommand(e) {
    if (e.ctrlKey && e.code === "KeyC") {
        console.log("Control C")
        // Ctrl+C - Отправляет сигнал SIGINT
        ws.send("cancel")
    }
}*/

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

// TODO create left and right handler for caret
    if (e.code === 'ArrowLeft' || e.code === 'ArrowRight') {

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
                connectWS(status)
                break
            case 'exit':
                disconnectWS()
                break
            case 'clear':
                list.innerHTML = ''
                break
            case 'helpConsole':
                commandResult = `
      'start' -  Connect to server CMD
      'exit'  -  Disconnect server CMD
      'clear' -  Clear WebConsole`
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
    if (command !== 'clear') {
        createList(command)
    }

    form.reset()
    updateState(status)
    handleInputStyle()
}
