mergeInto(LibraryManager.library, {
    // !!!!!!!!!!! WARNING !!!!!!!!!!!

    // DO NOT USE LET ASSIGNMENTS (ie `let x = 1`)
    // YOU WILL FAIL TO COMPILE DEEP IN THE EMSCRIPTEN PIPELINE
    // AND THE COMPILATION ERROR IS TERRIBLE AND CRYPTIC
    // FOR EXAMPLE: `js_parse_error(message, line, col, pos); ^typeerror: (intermediate value) is not a function`
    // WRAPPED IN A GIGANTIC UNFORMATTED STACK TRACE. GROSS. 

    // !!!!!!!!!!! WARNING !!!!!!!!!!!

    // Tip: run this through JSHint.com before building. Jslib in Unity uses ES5. Source: De-Panther. This does not seem to be in any official Unity documentation.

    // Tip: SendMessage can only send zero args, one number, or one string.

    // Tip: Check SocketIOJSLib for the definition of SUCCESS and FAILURE.

    // Init sync connections

    SetSocketIOAdapterName: function (nameBuffer) {
        if (nameBuffer == null) {
            console.error("SetSocketIOAdapterName: nameBuffer was null");
        }

        var _name = Pointer_stringify(nameBuffer);

        if (_name == null) {
            console.error("SetSocketIOAdapterName: name was null");
        }

        window.socketIOAdapterName = _name;
        
        //Return the socketIOAdapterName value so that Unity can check that it has the right value
        var bufferSize = lengthBytesUTF8(window.socketIOAdapterName) + 1;

        var buffer = _malloc(bufferSize);

        stringToUTF8(window.socketIOAdapterName, buffer, bufferSize);

        return buffer;
    },

    OpenSyncConnection: function () {
        window.socketIODebugInfo = {};

        // connect to socket.io relay server
        window.sync = io(window.RELAY_BASE_URL + "/sync");

        if (window.sync == null) {
            console.error("io(" + window.RELAY_BASE_URL + "/sync) failed");

            return 1;
        }

        window.socketIODebugInfo.relayBaseURL = window.RELAY_BASE_URL;

        console.log("====== sync ======:", socket);

        return 0;
    },

    OpenChatConnection: function () {
        window.chat = io(window.RELAY_BASE_URL + '/chat');

        if (window.chat == null) {
            console.error("io(" + window.RELAY_BASE_URL + "\'/chat\') failed");

            return 1;
        }

        return 0;
    },

    SetSyncEventListeners: function() {
        if (window.sync == null) {
            console.error("SetSyncEventListeners: window.sync was null");
        }

        if (window.gameInstance == null) {
            console.error("SetSyncEventListeners: window.gameInstance was null");
        }

        var sync = window.sync;
        
        var syncSocketId = (window.sync.id === undefined || window.sync.id == null) ? "No ID" : window.sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

        var networkManager = 'NetworkManager';

        if (window.socketIOAdapterName == null) {
            console.error("SetSyncEventListeners: window.socketIOAdapterName was null");
        }

        //source: https://socket.io/docs/v2/client-api/index.html

        sync.on('connect', function () {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + syncSocketId + "] Successfully connected to " + syncSocketId);
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnConnect', syncSocketId);
        });

        sync.on('serverName', function (serverName) {
            window.gameInstance.SendMessage(socketIOAdapter, 'OnServerName', serverName);
        });

        sync.on('disconnect', function (reason) {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + syncSocketId + "] Disconnected: " + reason);
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnDisconnect', reason);
        });

        sync.on('error', function (error) {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + syncSocketId + "] Error: " + error + ". Connected: " + sync.connected);
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnError', error);
        });

        sync.on('connect_error', function (error) {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + syncSocketId + "] Connect error: " + error);
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnConnectError', JSON.stringify(error)); //TODO(Brandon): continue changing sendmessage so that it sends to socketIOAdapter instead of networkManager or instantiationManager
        });

        sync.on('connect_timeout', function () {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + syncSocketId + "] Connect timeout.");
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnConnectTimeout');
        });

        sync.on('reconnect', function (attemptNumber) {
            //TODO -- fix these and the following functions to send more arguments. For some reason, syncSocketId and reason don't send, even when we use JSON.stringify() or reason.toString().

            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + syncSocketId + "]  Successfully reconnected on attempt number " + attemptNumber);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnReconnectSucceeded');
        });

        sync.on('reconnect_attempt', function(attemptNumber) { //identical to 'reconnecting' event
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + syncSocketId + "]  Reconnect attempt. Count: " + attemptNumber);
        });

        var socketIOAdapter = window.socketIOAdapterName;

        // NOTE(rob): If the sync gets disconnected, don't cache the updates.
        // Just purge the sendBuffer and resume the updates from current position. 
        sync.on('reconnecting', function(attemptNumber) {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            sync.sendBuffer = [];

            console.log("[SocketIO " + syncSocketId + "]  Reconnecting. Count: " + attemptNumber);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnReconnectAttempt', syncSocketId + "," + attemptNumber);
        });

        sync.on('reconnect_error', function (error) {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + syncSocketId + "]  Reconnect error: " + error + ".");

            window.gameInstance.SendMessage(socketIOAdapter, 'OnReconnectError', JSON.stringify(error));
        });

        sync.on('reconnect_failed', function () {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + syncSocketId + "]  Reconnect failed: specified maximum number of attempts exceeded.");

            window.gameInstance.SendMessage(socketIOAdapter, 'OnReconnectFailed');
        });

        sync.on('ping', function () {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + syncSocketId + "]  Ping.");

            window.gameInstance.SendMessage(socketIOAdapter, 'OnPing');
        });

        sync.on('pong', function (latency) {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + syncSocketId + "]  Pong: " + latency + "ms.");

            window.gameInstance.SendMessage(socketIOAdapter, 'OnPong', latency);
        });

        //Receive session info from the server. Request it with the SendSessionInfoRequest function. Komodo function.
        sync.on('sessionInfo', function (info) {
            var syncSocketId = (sync == null || sync.id === undefined || sync.id == null) ? "No ID" : sync.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.dir(info);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnSessionInfo', info);
        });
        
        // Handle when the server gives us a state catch-up event.
        sync.on('state', function(data) {
            console.log("[SocketIO " + syncSocketId + "] received state catch-up event:", data);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnReceiveStateCatchup', JSON.stringify(data));
        });

        // Handle when we are (or someone else is) successfully joined to a session.
        sync.on('joined', function(client_id) {
            console.log("[SocketIO " + syncSocketId + "] Joined: Client" + client_id);
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnClientJoined', client_id);
        });

        // Handle when we successfully joined a session.
        sync.on('successfullyJoined', function(session_id) {
            console.log("[SocketIO " + sync.id + "] Successfully joined session " + session_id);
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnSuccessfullyJoined', session_id);
        });

        // Handle when we failed to join a session.
        sync.on('failedToJoin', function(session_id, reason) {
            console.log("[SocketIO " + sync.id + "] Failed to join " + session_id + ": " + reason);
            
            window.gameInstance.SendMessage(socketIOAdapter, 'OnFailedToJoin', session_id);
        });
        
        // A client other than us left the session.
        sync.on('left', function(client_id) {
            console.log("[SocketIO " + syncSocketId + "] Left: Client" + client_id);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnOtherClientLeft', client_id);
        });
        
        // We failed to leave the session.
        sync.on('failedToLeave', function(session_id, reason) {
            console.log("[SocketIO " + syncSocketId + "] Failed to leave session" + session_id + ": " + reason);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnFailedToLeave', session_id);
        });
        
        // We successfully left the session.
        sync.on('successfullyLeft', function(session_id, reason) {
            console.log("[SocketIO " + syncSocketId + "] Successfully left session " + session_id);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnOwnClientLeft', session_id);
        });
        
        // A client other than us disconnected.
        sync.on('disconnected', function(client_id) {
            console.log("[SocketIO " + syncSocketId + "] Disconnected: Client" + client_id);

            window.gameInstance.SendMessage(socketIOAdapter, 'OnOtherClientDisconnected', client_id);
        });
        
        // Receive messages.
        sync.on('message', function (data) {
            if (!data) {
                console.warn("tried to receive message, but data was null");

                return;
            }

            var message = data.message;

            if (!message) {
                console.warn("tried to receive message, but data.message was null");

                return;
            }

            var type = data.type;

            if (!type) {
                console.warn("tried to receive message, but data.type was null");

                return;
            }

            var typeAndMessage = type + "|" + message;

            // call the Unity runtime "SendMessage" (unrelated to KomodoMessage stuff) routine to pass data to our "ProcessMessage" routine. 
            window.gameInstance.SendMessage(socketIOAdapter, 'OnMessage', typeAndMessage);
        });
        
        // Receive messages.
        sync.on('sendMessageFailed', function (reason) {
            console.error("send message failed: " + reason);

            // call the Unity runtime "SendMessage" (unrelated to KomodoMessage stuff) routine to pass data to our "ProcessMessage" routine. 
            window.gameInstance.SendMessage(socketIOAdapter, 'OnSendMessageFailed', reason);
        });
        
        // Handle the case where the server forcibly removed the socket.
        sync.on('bump', function (session_id) {
            console.log("You are logged in elsewhere. Bumped from the session.");

            window.gameInstance.SendMessage(socketIOAdapter, 'OnBump', session_id);
        });
    },

    JoinSyncSession: function () {
        if (window.sync == null ) {
            console.error("JoinSyncSession: window.sync was null");
            
            return 1;
        }

        if (window.session_id == null ) {
            console.error("JoinSyncSession: window.session_id was null");
            
            return 1;
        }

        if (window.client_id == null ) {
            console.error("JoinSyncSession: window.client_id was null");
            
            return 1;
        }
        
        var joinIds = [window.session_id, window.client_id];
        
        console.log("Asking relay to join session:", joinIds);
        
        sync.emit("join", joinIds);
        
        return 0;
    },

    JoinChatSession: function () {
        if (window.chat == null ) {
            console.error("JoinChatSession: window.chat was null");
            
            return 1;
        }

        if (window.session_id == null ) {
            console.error("JoinChatSession: window.session_id was null");
            
            return 1;
        }

        if (window.client_id == null ) {
            console.error("JoinChatSession: window.client_id was null");
            
            return 1;
        }
        
        var joinIds = [window.session_id, window.client_id];

        console.log("Asking relay to join chat:", joinIds);

        window.chat.emit("join", joinIds);
        
        return 0;
    },

    LeaveSyncSession: function () {
        if (window.sync == null ) {
            console.error("LeaveSyncSession: window.sync was null");
            
            return 1;
        }

        if (window.session_id == null ) {
            console.error("LeaveSyncSession: window.session_id was null");
            
            return 1;
        }

        if (window.client_id == null ) {
            console.error("LeaveSyncSession: window.client_id was null");
            
            return 1;
        }
        
        var joinIds = [window.session_id, window.client_id];
        
        console.log("Asking relay to leave session:", joinIds);
        
        sync.emit("leave", joinIds);
        
        return 0;
    },

    LeaveChatSession: function () {
        if (window.chat == null ) {
            console.error("LeaveChatSession: window.chat was null");

            return 1;
        }

        if (window.session_id == null ) {
            console.error("LeaveChatSession: window.session_id was null");

            return 1;
        }

        if (window.client_id == null ) {
            console.error("LeaveChatSession: window.client_id was null");

            return 1;
        }
        
        var joinIds = [window.session_id, window.client_id];

        console.log("Asking relay to leave chat:", joinIds);

        window.chat.emit("leave", joinIds);
        
        return 0;
    },

    /**
     * Asks the server to return a session object.
     */
    SendSessionInfoRequest: function () {
        if (window.sync == null) {
            console.error("SendSessionInfoRequest: window.sync was null");

            return 1;
        }

        if (window.session_id == null) {
            console.error("SendSessionInfoRequest: window.session_id was null");

            return 1;
        }

        window.sync.emit('sessionInfo', window.session_id);
        
        return 0;
    },

    SendStateCatchUpRequest: function() { 
        if (window.sync == null) {
            console.error("SendStateCatchUpRequest: window.sync was null");

            return 1;
        }

        if (window.session_id == null) {
            console.error("SendStateCatchUpRequest: window.session_id was null");

            return 1;
        }

        if (window.client_id == null) {
            console.error("SendStateCatchUpRequest: window.client_id was null");

            return 1;
        }
        
        window.sync.emit('state', { version: 2, session_id: session_id, client_id: client_id });
        
        return 0;
    },

    SetChatEventListeners: function () {
        if (window.chat == null) {
            console.error("SetChatEventListeners: window.sync was null");
            
            return 1;
        }

        if (window.gameInstance == null) {
            console.error("SetChatEventListeners: window.gameInstance was null");
            
            return 1;
        }

        var chat = window.chat;
        
        var chatId = (window.chat.id === undefined || window.chat.id == null) ? "No ID" : window.chat.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

        if (window.socketIOAdapterName == null) {
            console.error("SetChatEventListeners: window.socketIOAdapterName was null");

            return 1;
        }

        var socketIOAdapter = window.socketIOAdapterName;

        window.chat.on('micText', function(data) {
            gameInstance.SendMessage(socketIOAdapter, 'OnReceiveSpeechToTextSnippet', JSON.stringify(data));
        });
        
        return 0;
    },

    InitReceiveDraw: function(arrayPointer, size) {
        if (window.sync == null) {
            console.error("InitReceiveDraw: window.sync was null");

            return 1;
        }

        var drawCursor = 0;

        window.sync.on('draw', function(data) {
            if (data.length + drawCursor > size) {
                drawCursor = 0;
            }

            for (var i = 0; i < data.length; i++) {
                HEAPF32[(arrayPointer >> 2) + i + drawCursor] = data[i];
            }

            drawCursor += data.length;
        });
        
        return 0;
    },

    SendDraw: function (arrayPointer, size) {
        if (window.sync == null) {
            console.error("SendDraw: window.sync was null");

            return 1;
        }
            
        var drawSendBuff = [];

        for (var i = 0; i < size; i++) {
            drawSendBuff.push(HEAPF32[(arrayPointer >> 2) + i]);
        }

        window.sync.emit('draw', drawSendBuff);
        
        return 0;
    },

    GetClientIdFromBrowser: function() {
        return window.client_id;
    },

    GetSessionIdFromBrowser: function() {
        return window.session_id;
    },

    GetIsTeacherFlagFromBrowser: function() {
        return window.isTeacher;
    },

    SocketIOSendPosition: function (array, size) {
        if (window.sync == null) {
            console.error("SocketIOSendPosition: window.sync was null");

            return 1;
        }
            
        var posSendBuff = [];
            
        for (var i = 0; i < size; i++) {
            posSendBuff.push(HEAPF32[(array >> 2) + i]);
        }

        // timestamp the packet
        posSendBuff[size-1] = Date.now();
        
        window.sync.emit("update", posSendBuff);
        
        return 0;
    },
    
    SocketIOSendInteraction: function (array, size) {
        if (window.sync == null) {
            return 1;
        }

        var intSendBuff = [];

        for (var i = 0; i < size; i++) {
            intSendBuff.push(HEAP32[(array >> 2) + i]);
        }

        // timestamp the packet
        intSendBuff[size-1] = Date.now();
        
        window.sync.emit("interact", intSendBuff);
            
        return 0;
    },

    InitSocketIOReceivePosition: function(arrayPointer, size) {
        if (window.sync == null) {
            return 1;
        }

        var posCursor = 0;

        // NOTE(rob):
        // we use "arrayPointer >> 2" to change the pointer location on the module heap
        // when interpreted as float32 values ("HEAPF32[]"). 
        // for example, an original arrayPointer value (which is a pointer to a 
        // position on the module heap) of 400 would right shift to 100 
        // which would be the correct corresponding index on the heap
        // for elements of 32-bit size.

        window.sync.on('relayUpdate', function(data) {
            if (data.length + posCursor > size) {
                posCursor = 0;
            }

            for (var i = 0; i < data.length; i++) {
                HEAPF32[(arrayPointer >> 2) + posCursor + i] = data[i];
            }

            posCursor += data.length;
        });
        
        return 0;
    },
    
    InitSocketIOReceiveInteraction: function(arrayPointer, size) {
        if (window.sync == null) {
            return 1;
        }

        var intCursor = 0;

        window.sync.on('interactionUpdate', function(data) {
            if (data.length + intCursor > size) {
                intCursor = 0;
            }

            for (var i = 0; i < data.length; i++) {
                HEAP32[(arrayPointer >> 2) + intCursor + i] = data[i];
            }

            intCursor += data.length;
        });
        
        return 0;
    },

    ToggleCapture: function (operation, session_id) {
        if (window.sync == null) {
            return 1;
        }

        if (operation == 0) {
            window.sync.emit("start_recording", session_id);

            return 0;
        }
            
        window.sync.emit("end_recording", session_id);
        
        return 0;
    },

    GetSessionDetails: function() {
        var bufferSize;

        var buffer;

        if (window.details == null) {
            return null;
        }

        var serializedDetails = JSON.stringify(window.details);

        if (serializedDetails == null) {
            console.log("Unable to serialize details: " + window.details);
            
            bufferSize = lengthBytesUTF8("{}") + 1;
            
            buffer = _malloc(bufferSize);
            
            stringToUTF8("", buffer, bufferSize);
            
            return buffer;
        }
            
        bufferSize = lengthBytesUTF8(serializedDetails) + 1;
            
        buffer = _malloc(bufferSize);
            
        stringToUTF8(serializedDetails, buffer, bufferSize);
            
        return buffer;
    },

    EnableVRButton: function() {
        var button = document.getElementById('entervr');

        if (button == null) {
            console.error("No button with id `entervr` found.");

            return 1;
        }

        button.disabled = false;
        
        return 0;
    },

    // general messaging system
    BrowserEmitMessage: function (typePtr, messagePtr) {
        if (window.sync == null) {
            console.warn("BrowserEmitMessage: window.sync was null");

            return 1;
        }
            
        var type_str = Pointer_stringify(typePtr);
            
        var message_str = Pointer_stringify(messagePtr);
            
        window.sync.emit('message', {
            session_id: session_id,
            client_id: client_id,
            type: type_str,
            message: message_str,
            ts: Date.now()
        });
        
        return 0;
    },

    CloseSyncConnection: function () {
        if (window.sync == null) {
            console.error("Disconnect: window.sync was null");

            return 1;
        }
        
        window.sync.disconnect();

        return 0;
    },
    
    CloseChatConnection: function () {
        if (window.chat == null) {
            console.error("Disconnect: window.chat was null");

            return 1;
        }

        window.chat.disconnect();

        return 0;
    },
});
