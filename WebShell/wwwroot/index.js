// HTML Elements
const form = document.querySelector('#console')
const input = form.querySelector('#input')
const list = document.querySelector('.text')
const caret = document.querySelector('.caret')
const status = document.querySelector('#status')

// History
const loadHistory = () => JSON.parse(sessionStorage.getItem('command'))
const saveHistory = (history) => sessionStorage.setItem('command', JSON.stringify(history))
const history = loadHistory() || []
let flagHistory = false
let index = history.length ? history.length - 1 : 0

// Custom caret 
let inputLength
let indexCaret
let inputEdit = false
const moveCaret = (index) => caret.style.left = `calc(16ch + ${index}ch)`

// Blocking input
let canWrite = true

// Functions
const autoScroll = () => window.scrollTo(0, document.body.scrollHeight)
const inputFocus = () => input.focus()

function handleCancelCommand(e) {
    if (e.ctrlKey && e.code === "KeyC") {
        if (!canWrite) {
            ws.send("cancel")
        }
    }
}

/*function handleFocus() {
    input.focus()
}*/

/*function loadHistory() {
    return JSON.parse(sessionStorage.getItem('command'))
}

function saveHistory(history) {
    sessionStorage.setItem('command', JSON.stringify(history))
}

function moveCaret(index) {
    caret.style.left = `calc(16ch + ${index}ch)`
}*/

function handleInputCaret(e) {
    inputLength = input.value.length

    if (inputEdit) {
        if (e?.inputType === "insertText") {
            indexCaret++
        } else {
            indexCaret--
        }
    } else {
        indexCaret = inputLength
    }
    moveCaret(indexCaret)
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
    if (e.code === 'ArrowLeft' || e.code === 'ArrowRight') {
        
        if (e.code === 'ArrowLeft') {
            if (indexCaret === 0) return
            inputEdit = true
            indexCaret--
            // moveCaret(indexCaret)
        } else {
            if (inputLength === indexCaret) return inputEdit = false
            indexCaret++
        }
        moveCaret(indexCaret)
    }

    if (!history.length) return

    if (e.code === 'ArrowUp' || e.code === 'ArrowDown') {
        inputEdit = false
        if (e.code === 'ArrowUp') {
            e.preventDefault()
            keyArrowUp()
        } else if (flagHistory) {
            keyArrowDown()
        }

        input.value = history[index]
        handleInputCaret()
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
            case 'helpshell':
                commandResult = `
      'start'       -   Connect to server CMD
      'exit'        -   Disconnect server CMD
      'clear'       -   Clear WebConsole
      'helpshell'   -   Help`
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
    updateState()
    handleInputCaret()
    inputFocus()
}

// Events
form.addEventListener('submit', handleForm)
input.addEventListener('keydown', handleKey)
input.addEventListener('input', handleInputCaret)
document.addEventListener('click', inputFocus)
document.addEventListener('mouseenter', () => caret.classList.remove('hide'))
document.addEventListener('mouseleave', () => caret.classList.add('hide'))
document.addEventListener('keydown', handleCancelCommand)