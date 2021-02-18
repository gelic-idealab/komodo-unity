mergeInto(LibraryManager.library, {


    // !!!!!!!!!!! WARNING !!!!!!!!!!!

    // DO NOT USE LET ASSIGNMENTS (ie `let x = 1`)
    // YOU WILL FAIL TO COMPILE DEEP IN THE EMSCRIPTEN PIPELINE
    // AND THE COMPILATION ERROR IS TERRIBLE AND CRYPTIC
    // FOR EXAMPLE: `js_parse_error(message, line, col, pos); ^typeerror: (intermediate value) is not a function`
    // WRAPPED IN A GIGANTIC UNFORMATTED STACK TRACE. GROSS. 

    // !!!!!!!!!!! WARNING !!!!!!!!!!!


    // Init socket connections

    InitSocketConnection: function() {
        // connect to socket.io relay server
        window.socket = io(window.RELAY_BASE_URL);
        console.log("====== SOCKET ======:", socket);

        // NOTE(rob): If the socket gets disconnected, don't cache the updates.
        // Just purge the sendBuffer and resume the updates from current position. 
        socket.on('reconnecting',function(){
            socket.sendBuffer = [];
        });

        // Join the session.
        var joinIds = [window.session_id, window.client_id]
        console.log("Asking relay to join session:", joinIds);
        socket.emit("join", joinIds);

        // text chat relay
        chat = io(window.RELAY_BASE_URL + '/chat');
        chat.emit("join", joinIds);
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
            window.socket.on('state', function(data) {
                console.log('received state sync event:', data);
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
            window.socket.on('joined', function(client_id) {
                window.gameInstance.SendMessage('NetworkManager','RegisterNewClientId', client_id);
            });
        }
    },

    InitClientDisconnectHandler: function () {
        if (window.socket) {
            window.socket.on('disconnected', function(client_id) {
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
            window.socket.emit("update", posSendBuff);
        }
    },

    SocketIOSendInteraction: function (array, size) {
        if (window.socket) {
            var intSendBuff = [];
            for (var i = 0; i < size; i++) {
                intSendBuff.push(HEAP32[(array >> 2) + i]);
            }
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
            var str = Pointer_stringify(ptr);
            var message = JSON.parse(str);
            console.log('message send:', message);
            window.socket.emit('message', {
                session_id: session_id,
                client_id: client_id,
                message: message
            });
        }
    },

    InitBrowserReceiveMessage: function () {
        if (window.socket) {
            console.log('InitBrowserReceiveMessage');
            window.socket.on('message', function (data) {
                console.log('message receive:', data);
                var message = data.message;
                window.gameInstance.SendMessage("NetworkManager", 'ProcessMessage', JSON.stringify(message));
            });
        }
    }
});
