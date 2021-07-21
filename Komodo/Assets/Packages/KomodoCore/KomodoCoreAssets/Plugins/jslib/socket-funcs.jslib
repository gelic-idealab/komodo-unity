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

    // Init socket connections

    InitSocketConnection: function() {
        window.socketIODebugInfo = {};

        // connect to socket.io relay server
        window.socket = io(window.RELAY_BASE_URL);

        window.socketIODebugInfo.relayBaseURL = window.RELAY_BASE_URL;

        console.log("====== SOCKET ======:", socket);

        // NOTE(rob): If the socket gets disconnected, don't cache the updates.
        // Just purge the sendBuffer and resume the updates from current position. 
        socket.on('reconnecting', function(attemptNumber) {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            socket.sendBuffer = [];

            console.log("[SocketIO " + socketId + "]  Reconnecting. Count: " + attemptNumber);

            window.gameInstance.SendMessage('NetworkManager', 'OnReconnectAttempt', socketId + "," + attemptNumber);
        });

        //source: https://socket.io/docs/v2/client-api/index.html

        socket.on('reconnect_attempt', function(attemptNumber) { //identical to 'reconnecting' event
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + socketId + "]  Reconnect attempt. Count: " + attemptNumber);
        });

        socket.on('reconnect', function (attemptNumber) {
            //TODO -- fix these and the following functions to send more arguments. For some reason, socketId and reason don't send, even when we use JSON.stringify() or reason.toString().

            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + socketId + "]  Successfully reconnected on attempt number " + attemptNumber);

            window.gameInstance.SendMessage('NetworkManager', 'OnReconnectSucceeded');
        });

        socket.on('reconnect_error', function (error) {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + socketId + "]  Reconnect error: " + error + ".");

            window.gameInstance.SendMessage('NetworkManager', 'OnReconnectError', JSON.stringify(error));
        });

        socket.on('reconnect_failed', function () {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + socketId + "]  Reconnect failed: specified maximum number of attempts exceeded.");

            window.gameInstance.SendMessage('NetworkManager', 'OnReconnectFailed');
        });

        socket.on('connect', function () {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments

            console.log("[SocketIO " + socketId + "] Successfully connected to " + socketId);
            
            window.gameInstance.SendMessage('NetworkManager', 'OnConnect', socketId);
        });

        socket.on('connect_error', function (error) {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + socketId + "] Connect error: " + error);
            
            window.gameInstance.SendMessage('NetworkManager', 'OnConnectError', JSON.stringify(error));
        });

        socket.on('connect_timeout', function () {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + socketId + "] Connect timeout.");
            
            window.gameInstance.SendMessage('NetworkManager', 'OnConnectTimeout');
        });

        socket.on('disconnect', function (reason) {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + socketId + "] Disconnected: " + reason);
            
            window.gameInstance.SendMessage('NetworkManager', 'OnDisconnect', reason);
        });

        socket.on('error', function (error) {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.log("[SocketIO " + socketId + "] Error: " + error + ". Connected: " + socket.connected);
            
            window.gameInstance.SendMessage('NetworkManager', 'OnError');
        });

        //Receive session info from the server. Request it with the GetSessionInfo function.
        socket.on('sessionInfo', function (info) {
            var socketId = (socket == null || socket.id === undefined || socket.id == null) ? "No ID" : socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            console.dir(info);

            window.gameInstance.SendMessage('NetworkManager', 'OnSessionInfo');
        });

        // Join the session.
        var joinIds = [window.session_id, window.client_id];
        console.log("Asking relay to join session:", joinIds);
        socket.emit("join", joinIds);

        // text chat relay
        chat = io(window.RELAY_BASE_URL + '/chat');
        chat.emit("join", joinIds);
    },

    /**
     * Asks the server to return a session object.
     */
    GetSessionInfo: function () {
        if (window.socket) {
            window.socket.emit('sessionInfo', window.session_id);
        }
    },

    // // const startPlayback = function() {
    // //     console.log('playback started:', playback_id);
    // //     let playbackArgs = [client_id, session_id, playback_id]
    // //     socket.emit('playback', playbackArgs);
    // // }

    // socket.on('playbackEnd', function() {
    //     console.log('playback ended');
    // });

    InitSessionStateHandler: function() {
        if (window.socket) {
            var socketId = (window.socket.id === undefined || window.socket.id == null) ? "No ID" : window.socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            window.socket.on('state', function(data) {
                console.log("[SocketIO " + socketId + "] received state sync event:", data);

                gameInstance.SendMessage('InstantiationManager', 'SyncSessionState', JSON.stringify(data));
            });
        }
    },

    InitSessionState: function() {
        if (window.socket) {
            window.socket.emit('state', { version: 2, session_id: session_id, client_id: client_id });
        }
    },

    InitSocketIOClientCounter: function() {
        if (window.socket) {
            var socketId = (window.socket.id === undefined || window.socket.id == null) ? "No ID" : window.socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            window.socket.on('joined', function(client_id) {
                console.log("[SocketIO " + socketId + "] Joined: Client" + client_id);
                
                window.gameInstance.SendMessage('NetworkManager','RegisterNewClientId', client_id);
            });
        }
    },

    InitClientDisconnectHandler: function () {
        if (window.socket) {
            var socketId = (window.socket.id === undefined || window.socket.id == null) ? "No ID" : window.socket.id; //do this so we can call sendMessage without it accidentally interpreting null as the end of the arguments
            
            window.socket.on('disconnected', function(client_id) {
                console.log("[SocketIO " + socketId + "] Disconnected: Client" + client_id);

                window.gameInstance.SendMessage('NetworkManager','UnregisterClientId', client_id);
            });
        }
    },

    InitMicTextHandler: function () {
        if (window.chat) {
            window.chat.on('micText', function(data) {
                console.log('micText:', data);
                gameInstance.SendMessage('InstantiationManager', 'Text_Refresh', JSON.stringify(data));
            });
        }
    },

    InitReceiveDraw: function(arrayPointer, size) {
        if (window.socket) {
            var drawCursor = 0;
            window.socket.on('draw', function(data) {
                if (data.length + drawCursor > size) {
                    drawCursor = 0;
                }
                for (var i = 0; i < data.length; i++) {
                    HEAPF32[(arrayPointer >> 2) + i + drawCursor] = data[i];
                }
                drawCursor += data.length;
            });
        }
    },

    SendDraw: function (arrayPointer, size) {
        if (window.socket) {
            var drawSendBuff = [];
            for (var i = 0; i < size; i++) {
                drawSendBuff.push(HEAPF32[(arrayPointer >> 2) + i]);
            }
            window.socket.emit('draw', drawSendBuff);
        }
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
        if (window.socket) {
            var posSendBuff = [];
            for (var i = 0; i < size; i++) {
                posSendBuff.push(HEAPF32[(array >> 2) + i]);
            }
            // timestamp the packet
            posSendBuff[size-1] = Date.now();
            window.socket.emit("update", posSendBuff);
        }
    },
    
    SocketIOSendInteraction: function (array, size) {
        if (window.socket) {
            var intSendBuff = [];
            for (var i = 0; i < size; i++) {
                intSendBuff.push(HEAP32[(array >> 2) + i]);
            }
            // timestamp the packet
            intSendBuff[size-1] = Date.now();
            window.socket.emit("interact", intSendBuff);
        }
    },

    InitSocketIOReceivePosition: function(arrayPointer, size) {
        if (window.socket) {
            var posCursor = 0;

            // NOTE(rob):
            // we use "arrayPointer >> 2" to change the pointer location on the module heap
            // when interpreted as float32 values ("HEAPF32[]"). 
            // for example, an original arrayPointer value (which is a pointer to a 
            // position on the module heap) of 400 would right shift to 100 
            // which would be the correct corresponding index on the heap
            // for elements of 32-bit size.

            window.socket.on('relayUpdate', function(data) {
                if (data.length + posCursor > size) {
                    posCursor = 0;
                }
                for (var i = 0; i < data.length; i++) {
                    HEAPF32[(arrayPointer >> 2) + posCursor + i] = data[i];
                }
                posCursor += data.length;
            });
        }
    },
    
    InitSocketIOReceiveInteraction: function(arrayPointer, size) {
        if (window.socket) {
            var intCursor = 0;
            window.socket.on('interactionUpdate', function(data) {
                if (data.length + intCursor > size) {
                    intCursor = 0;
                }
                for (var i = 0; i < data.length; i++) {
                    HEAP32[(arrayPointer >> 2) + intCursor + i] = data[i];
                }
                intCursor += data.length;
            });
        }
    },

    Record_Change: function (operation, session_id) {
        if (window.socket) {
            if (operation == 0) {
                window.socket.emit("start_recording",session_id);
            } 
            else {
                window.socket.emit("end_recording",session_id);
            }
        }
    },

    GetSessionDetails: function() {
        if (window.details) {
            var serializedDetails = JSON.stringify(window.details);
            if (serializedDetails) {
                var bufferSize = lengthBytesUTF8(serializedDetails) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(serializedDetails, buffer, bufferSize);
                return buffer;
            } else {
                console.log("Unable to serialize details: " + window.details)
                var bufferSize = lengthBytesUTF8("{}") + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8("", buffer, bufferSize);
                return buffer;
            }
        } else {
            // var bufferSize = lengthBytesUTF8("{details:{}}") + 1;
            // var buffer = _malloc(bufferSize);
            // stringToUTF8("", buffer, bufferSize);
            // return buffer;
            return null;
        }
    },

    EnableVRButton: function() {
       var button = document.getElementById('entervr');
       button.disabled = false;
    },

    // general messaging system
    BrowserEmitMessage: function (ptr) {
        if (window.socket) {
            var message_str = Pointer_stringify(ptr);
            window.socket.emit('message', {
                session_id: session_id,
                client_id: client_id,
                message: message_str,
                ts: Date.now()
            });
        }
    },

    InitBrowserReceiveMessage: function () {
        if (window.socket) {
            console.log('InitBrowserReceiveMessage');
            window.socket.on('message', function (data) {
                var message = data.message;
                window.gameInstance.SendMessage("NetworkManager", 'ProcessMessage', message);
            });
        }
    },

    Disconnect: function () {
        if (window.socket) {
            window.socket.disconnect();
        }

        if (window.chat) {
            window.chat.disconnect();
        }
    }
});
