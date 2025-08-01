mergeInto(LibraryManager.library, {
  WebSocketConnect: function (urlPtr, gameObjectNamePtr, callbackMethodPtr) {
    const url = UTF8ToString(urlPtr);
    const gameObjectName = UTF8ToString(gameObjectNamePtr);
    const callbackMethod = UTF8ToString(callbackMethodPtr);

    if (typeof Module.webSocket !== "undefined" && Module.webSocket.readyState !== WebSocket.CLOSED) {
      Module.webSocket.close();
    }

    Module.webSocket = new WebSocket(url);

    Module.webSocket.onopen = function () {
      SendMessage(gameObjectName, callbackMethod, JSON.stringify({ evt: "open", data: "" }));
      console.log("Open");
    };

    Module.webSocket.onmessage = function (msg) {
      SendMessage(gameObjectName, callbackMethod, JSON.stringify({ evt: "receive", data: msg.data }));
      //console.log("Message");
    };

    Module.webSocket.onclose = function () {
      SendMessage(gameObjectName, callbackMethod, JSON.stringify({ evt: "close", data: "" }));
      console.log("Close");
    };

    Module.webSocket.onerror = function (err) {
      SendMessage(gameObjectName, callbackMethod, JSON.stringify({ evt: "error", data: err }));
      console.log("Error");
    };
  },

  WebSocketSend: function (messagePtr) {
    const message = UTF8ToString(messagePtr);
    if (Module.webSocket && Module.webSocket.readyState === WebSocket.OPEN) {
      Module.webSocket.send(message);
      //console.log("Send");
    }
    else{
    console.log("Send Error");
    }
  },

  WebSocketClose: function () {
    if (Module.webSocket) {
      Module.webSocket.close();
      console.log("Closing");
    }
  }
});
