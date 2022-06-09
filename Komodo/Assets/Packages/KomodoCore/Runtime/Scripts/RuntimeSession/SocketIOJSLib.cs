using System.Runtime.InteropServices;

namespace Komodo.Runtime
{
    /// <summary>
    /// SocketIOJSLib.cs is a script that has its methods implemented by an external .jslib file, that is, socket-funcs.lib. These two files are essential for understanding how C# and jslib (which is written in JavaScript) works together. The key term being used in this script is the <c>[DllImport("__Internal")]</c>. This is a necessary attribute for methods written in C# in this script to call functions from external libraries.
    /// </summary>
    public static class SocketIOJSLib
    {
        public static int SUCCESS = 0;

        public static int FAILURE = 1;

        // import callable js functions
        // socket.io with webgl
        // https://www.gamedev.net/articles/programming/networking-and-multiplayer/integrating-socketio-with-unity-5-webgl-r4365/

        /// <summary>
        /// A C# string will get passed in to the method, and the method will check and convert the string to a JavaScript string. The converting process invovles bytes computation by using some functions like <code>lengthBytesUTF8()</code>.
        /// </summary>
        /// <param name="name">A C# string; be careful with this distinction: C# strings are different from JavaScript strings. For the implementation, we used <c>Pointer_Stringify()</c> to convert a C# string to a JavaScript string.</param> 
        /// <returns></returns>
        [DllImport("__Internal")]
        public static extern string SetSocketIOAdapterName(string name);


        /// <summary>
        /// This method establishes connections between the clients and the server. In other words, there are event listeners that recieve an event emit and response with certain actions. 
        /// </summary>
        [DllImport("__Internal")]
        public static extern int SetSyncEventListeners();


        /// <summary>
        /// Connect socket.io to the relay server.
        /// </summary>
        /// <returns>This returns either 1 or 0. Returning 0 means that sync connection has been established</returns>
        [DllImport("__Internal")]
        public static extern int OpenSyncConnection();


        /// <summary>
        /// Returns 0 if chat connection has been established successfully.
        /// </summary>
        /// <returns>This returns either 1 or 0. Returning 0 means that chat connection has been established; 1 for otherwise.</returns>
        [DllImport("__Internal")]
        public static extern int OpenChatConnection();


        /// <summary>
        /// Checks if sync, session_id, and client_id are null. If so, return 1 meaning that it does not join the session successfully. Otherwise, return 0.
        /// </summary>
        /// <returns>Return 0 if user has joined the session successfully.</returns>
        [DllImport("__Internal")]
        public static extern int JoinSyncSession();


        /// <summary>
        /// The implementation is almost identical to <code>JoinSyncSession()</code> 
        /// </summary>
        /// <returns>Return 0 if user has joined the chat session successfully; otherwise, return 1.</returns>
        [DllImport("__Internal")]
        public static extern int JoinChatSession();


        /// <summary>
        /// Checks if sync, session_id, and client_id are null. If not, returns 0 and emits an event that has information of the current state to clients. 
        /// </summary>
        /// <returns>Return 1 if some information is null. Return 0 if event is emitted successfully.</returns>
        [DllImport("__Internal")]
        public static extern int SendStateCatchUpRequest();


        /// <summary>
        /// If all the information (being chat.id, window.chat, and etc.) is valid, set up an event listener on the client's side that chats.
        /// </summary>
        /// <returns>Return 1 if some information is undefined or null.</returns>
        [DllImport("__Internal")]
        public static extern int SetChatEventListeners();


        /// <summary>
        /// This returns the current client's id.
        /// </summary>
        [DllImport("__Internal")]
        public static extern int GetClientIdFromBrowser();

        /// <summary>
        /// This returns the current session's id.
        /// </summary>
        [DllImport("__Internal")]
        public static extern int GetSessionIdFromBrowser();


        /// <summary>
        /// This returns 1 if the current user is a teacher; otherwise, returns 0.
        /// </summary>
        [DllImport("__Internal")]
        public static extern int GetIsTeacherFlagFromBrowser();

        // [DllImport("__Internal")]
        // private static extern void InitSocketIOReceivePosition(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SocketIOSendPosition(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SocketIOSendInteraction(int[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void InitSocketIOReceiveInteraction(int[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void InitReceiveDraw(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SendDraw(float[] array, int size);


        /// <summary>
        /// This enables the button that switches to VR mode in a Komodo session.
        /// </summary>
        /// Return 0 if the button is successfully turned on.
        [DllImport("__Internal")]
        public static extern int EnableVRButton();

        /// <summary>
        /// Return current session's details.
        /// </summary>
        [DllImport("__Internal")]
        public static extern string GetSessionDetails();

        /// <summary>
        /// This emits a message that contains session information to the server.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        [DllImport("__Internal")]
        public static extern void BrowserEmitMessage(string type, string message); // TODO(rob): move this to GlobalMessageManager.cs

        /// <summary>
        /// This method emits a message--with information of the current user--to the server, asking for a permission to leave the current session.
        /// </summary>
        /// <returns>Return 1 if either session id or client id is null; return 0 when the leave request is sent by the client.</returns>
        [DllImport("__Internal")]
        public static extern int LeaveSyncSession();


        /// <summary>
        /// This is similar to the <code>LeaveSyncSession()</code>, except it emits a leave request from the client to the server. 
        /// </summary>
        [DllImport("__Internal")]
        public static extern int LeaveChatSession();


        /// <summary>
        /// Disconnect from the session.
        /// </summary>
        [DllImport("__Internal")]
        public static extern int CloseSyncConnection();

        /// <summary>
        /// Close the chat connection.
        /// </summary>
        /// <returns>Return 1 if it fails to close the chat connection due to whatever reason. Otherwise return 0 if the chat is closed successfully.</returns>
        [DllImport("__Internal")]
        public static extern int CloseChatConnection();
    }
}