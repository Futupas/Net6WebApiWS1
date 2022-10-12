'use strict';

async function main() {
    const res = await fetch('/data');
    const json = await res.json();
    const code = json.wsCode;
    console.log(`Got code ${code}`);

    const socket = new WebSocket(`wss://${window.location.hostname}:${window.location.port}/ws?code=${code}`);

    socket.onopen = function(e) {
        console.log("[open] Соединение установлено");
        console.log("Отправляем данные на сервер");
        socket.send("Меня зовут Джон");
    };
      
    socket.onmessage = function(event) {
        console.log(`[message] Данные получены с сервера: ${event.data}`);
    };
      
    socket.onclose = function(event) {
        if (event.wasClean) {
            console.log(`[close] Соединение закрыто чисто, код=${event.code} причина=${event.reason}`);
        } else {
            // например, сервер убил процесс или сеть недоступна
            // обычно в этом случае event.code 1006
            console.log('[close] Соединение прервано');
        }
    };
      
    socket.onerror = function(error) {
        console.error(`[error] ${error.message}`);
    };
      
}

main();