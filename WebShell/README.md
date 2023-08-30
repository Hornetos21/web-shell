# Web-Shell

![webShell.jpg](..%2FwebShell.jpg)

## Requirements
Необходимо реализовать веб приложение с функционалом:

- Позволяет ввести консольную команду в текстовое поле на веб странице и отправить её на сервер для выполнения. Команда должна выполняться в любом шелле, который есть в системе (cmd, powershell, bash (на ваш выбор)), либо без шелла вовсе (например запуск консольного приложения (git, ping и т.д.) и вывод его вывода).
- Получить вывод этой команды на сервере (стандартный вывод или вывод ошибки) и отобразить на веб странице
- Сохранить историю введенных команд

## Stack
- .NET 7
- WebSocket
- JS

## Run project

- Clone repository
- Open on your IDE
- Push the RUN button
- Enter command `start` in opened browser
- Client connecting to server
- Enter standard commands like `dir` or `time /t`
- To finish work with web shell enter `exit`

### Internal commands:
| command     | description       |
|-------------|:------------------|
| `start`     | Connect to server |
| `exit`      | Disconnect server |
| `helpShell` | Help              |
| `clear`     | Clear web shell   |
